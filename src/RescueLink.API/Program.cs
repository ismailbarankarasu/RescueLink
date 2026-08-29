using RescueLink.API.Common;
using RescueLink.API.Extensions;
using RescueLink.API.Services;
using RescueLink.Application;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Infrastructure;
using RescueLink.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddApiSerilog();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddLocalizedJwtResponses();

builder.Services.AddPersistence(
    builder.Configuration);

builder.Services.AddApiServices();

builder.Services.AddFrontendCors(
    builder.Configuration);

builder.Services.AddApiRateLimiting(builder.Configuration);
builder.Services.AddApiHealthChecks();
builder.Services.AddApiLocalization();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();

app.UseApiRequestLogging();
app.UseRequestLocalization();
app.UseExceptionHandler();

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

app.MapApiHealthChecks();
app.MapControllers();

app.Run();

public partial class Program;