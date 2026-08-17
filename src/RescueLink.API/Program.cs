using RescueLink.API.Common;
using RescueLink.API.ExceptionHandlers;
using RescueLink.API.Services;
using RescueLink.Application;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Infrastructure;
using RescueLink.Persistence;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
var app = builder.Build();

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

app.MapControllers();

app.Run();
