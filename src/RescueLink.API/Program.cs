using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RescueLink.API.Common;
using RescueLink.API.ExceptionHandlers;
using RescueLink.API.Services;
using RescueLink.Application;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Infrastructure;
using RescueLink.Persistence;
using RescueLink.Persistence.Context;
using Serilog;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(
                context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    });

// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();
builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddPersistence(
    builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });
var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? [];

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "At least one CORS origin must be configured.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        CorsPolicies.Frontend,
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (
        context,
        cancellationToken) =>
    {
        context.HttpContext.Response.ContentType =
            "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                code = "RateLimit.Exceeded",
                message =
                    "Too many requests. Please try again later."
            },
            cancellationToken);
    };

    options.AddPolicy(
        RateLimitPolicies.Authentication,
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection
                        .RemoteIpAddress?
                        .ToString()
                    ?? "unknown",
                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

    options.AddPolicy(
        RateLimitPolicies.Token,
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection
                        .RemoteIpAddress?
                        .ToString()
                    ?? "unknown",
                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
});

builder.Services.AddOpenApi();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<RescueLinkDbContext>(
        name: "sql-server",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("tr"),
    new CultureInfo("de")
};

builder.Services.Configure<RequestLocalizationOptions>(
    options =>
    {
        options.DefaultRequestCulture =
            new RequestCulture("en");

        options.SupportedCultures =
            supportedCultures;

        options.SupportedUICultures =
            supportedCultures;

        options.ApplyCurrentCultureToResponseHeaders =
            true;

        options.RequestCultureProviders =
        [
            new AcceptLanguageHeaderRequestCultureProvider()
        ];
    });

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();

if (app.Configuration.GetValue<bool>(
    "Database:ApplyMigrations"))
{
    await using var scope =
        app.Services.CreateAsyncScope();

    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<RescueLinkDbContext>();

    await dbContext.Database.MigrateAsync();
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} " +
        "in {Elapsed:0.0000} ms | TraceId: {TraceId} | UserId: {UserId}";

    options.GetLevel = (
        httpContext,
        elapsed,
        exception) =>
    {
        if (exception is not null ||
            httpContext.Response.StatusCode >= 500)
        {
            return Serilog.Events.LogEventLevel.Error;
        }

        if (httpContext.Response.StatusCode >= 400)        {
            return Serilog.Events.LogEventLevel.Warning;
        }

        return Serilog.Events.LogEventLevel.Information;
    };

    options.EnrichDiagnosticContext = (
        diagnosticContext,
        httpContext) =>
    {
        var userId = httpContext.User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? "Anonymous";

        diagnosticContext.Set(
            "TraceId",
            httpContext.TraceIdentifier);

        diagnosticContext.Set(
            "UserId",
            userId);
    };
});
app.UseRequestLocalization();
app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(CorsPolicies.Frontend);
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration =>
            registration.Tags.Contains("ready")
    });

app.MapControllers();
app.MapControllers();

app.Run();
public partial class Program;