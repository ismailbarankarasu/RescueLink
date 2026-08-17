using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using RescueLink.API.ExceptionHandlers;

namespace RescueLink.API.Tests.ExceptionHandlers;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ShouldReturnSafeProblemDetails()
    {
        // Arrange
        var loggerMock =
            new Mock<ILogger<GlobalExceptionHandler>>();

        var handler = new GlobalExceptionHandler(
            loggerMock.Object);

        var httpContext = new DefaultHttpContext();

        httpContext.TraceIdentifier =
            "test-trace-id";

        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/test";
        httpContext.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException(
            "Sensitive database information.");

        // Act
        var handled = await handler.TryHandleAsync(
            httpContext,
            exception,
            CancellationToken.None);

        // Assert
        handled.Should().BeTrue();

        httpContext.Response.StatusCode.Should().Be(
            StatusCodes.Status500InternalServerError);

        httpContext.Response.Body.Position = 0;

        using var responseJson =
            await JsonDocument.ParseAsync(
                httpContext.Response.Body);

        var root = responseJson.RootElement;

        root.GetProperty("status")
            .GetInt32()
            .Should()
            .Be(StatusCodes.Status500InternalServerError);

        root.GetProperty("title")
            .GetString()
            .Should()
            .Be("An unexpected error occurred.");

        root.GetProperty("traceId")
            .GetString()
            .Should()
            .Be("test-trace-id");

        root.GetProperty("instance")
            .GetString()
            .Should()
            .Be("/api/test");

        root.GetRawText()
            .Should()
            .NotContain("Sensitive database information");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldLogException()
    {
        // Arrange
        var loggerMock =
            new Mock<ILogger<GlobalExceptionHandler>>();

        var handler = new GlobalExceptionHandler(
            loggerMock.Object);

        var httpContext = new DefaultHttpContext();

        httpContext.TraceIdentifier =
            "exception-trace-id";

        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/api/test";
        httpContext.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException(
            "Test exception.");

        // Act
        await handler.TryHandleAsync(
            httpContext,
            exception,
            CancellationToken.None);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (value, type) =>
                        value.ToString()!.Contains(
                            "exception-trace-id")),
                exception,
                It.IsAny<Func<
                    It.IsAnyType,
                    Exception?,
                    string>>()),
            Times.Once);
    }
}