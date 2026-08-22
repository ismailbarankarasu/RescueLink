using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using RescueLink.API.Common;
using RescueLink.API.Extensions;
using RescueLink.Application.Common.Results;

namespace RescueLink.API.Tests.Extensions;

public sealed class JwtBearerExtensionsTests
{
    [Fact]
    public async Task OnForbidden_ShouldReturnLocalizedJsonResponse()
    {
        // Arrange
        var localizedError = new Error(
            "Authorization.Forbidden",
            "Bu işlemi gerçekleştirme yetkiniz yok.");

        var errorLocalizerMock =
            new Mock<IErrorLocalizer>();

        errorLocalizerMock
            .Setup(localizer => localizer.Localize(
                It.Is<Error>(error =>
                    error.Code ==
                    "Authorization.Forbidden")))
            .Returns(localizedError);

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton<IErrorLocalizer>(
            errorLocalizerMock.Object);

        services
            .AddAuthentication(
                JwtBearerDefaults
                    .AuthenticationScheme)
            .AddJwtBearer();

        services.AddLocalizedJwtResponses();

        using var serviceProvider =
            services.BuildServiceProvider();

        var jwtOptions =
            serviceProvider
                .GetRequiredService<
                    IOptionsMonitor<
                        JwtBearerOptions>>()
                .Get(
                    JwtBearerDefaults
                        .AuthenticationScheme);

        var httpContext =
            new DefaultHttpContext
            {
                RequestServices =
                    serviceProvider
            };

        await using var responseBody =
            new MemoryStream();

        httpContext.Response.Body =
            responseBody;

        var authenticationScheme =
            new AuthenticationScheme(
                JwtBearerDefaults
                    .AuthenticationScheme,
                displayName: null,
                handlerType:
                    typeof(JwtBearerHandler));

        var forbiddenContext =
            new ForbiddenContext(
                httpContext,
                authenticationScheme,
                jwtOptions);

        // Act
        await jwtOptions.Events.Forbidden(
            forbiddenContext);

        // Assert
        httpContext.Response.StatusCode
            .Should()
            .Be(StatusCodes.Status403Forbidden);

        httpContext.Response.ContentType
            .Should()
            .StartWith("application/json");

        responseBody.Position = 0;

        using var responseJson =
            await JsonDocument.ParseAsync(
                responseBody);

        var response =
            responseJson.RootElement;

        response.GetProperty("code")
            .GetString()
            .Should()
            .Be("Authorization.Forbidden");

        response.GetProperty("message")
            .GetString()
            .Should()
            .Be(
                "Bu işlemi gerçekleştirme yetkiniz yok.");

        errorLocalizerMock.Verify(
            localizer => localizer.Localize(
                It.Is<Error>(error =>
                    error.Code ==
                    "Authorization.Forbidden")),
            Times.Once);
    }
}