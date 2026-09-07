using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TodoListBackend.Exceptions;
using TodoListBackend.Middlewares;
using Xunit;

namespace TodoListBackend.Tests.Middlewares;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task BusinessException_ReturnsProblemDetailsWithStableCode()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(
            _ => throw new BusinessException("Dữ liệu không hợp lệ.", code: "invalid_test_data"));

        await middleware.InvokeAsync(context);

        var response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Equal("invalid_test_data", response.RootElement.GetProperty("code").GetString());
        Assert.Equal("Dữ liệu không hợp lệ.", response.RootElement.GetProperty("detail").GetString());
        Assert.Equal(context.TraceIdentifier, response.RootElement.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task UnhandledException_ReturnsGenericProblemWithoutLeakingDetails()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(
            _ => throw new InvalidOperationException("Database password: highly-sensitive"));

        await middleware.InvokeAsync(context);

        var response = await ReadResponseAsync(context);
        var serializedResponse = response.RootElement.GetRawText();

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("internal_server_error", response.RootElement.GetProperty("code").GetString());
        Assert.DoesNotContain("highly-sensitive", serializedResponse, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnauthorizedException_DoesNotReturnTheOriginalExceptionMessage()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(
            _ => throw new UnauthorizedAccessException("Internal authentication implementation detail"));

        await middleware.InvokeAsync(context);

        var response = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("unauthorized", response.RootElement.GetProperty("code").GetString());
        Assert.DoesNotContain(
            "Internal authentication implementation detail",
            response.RootElement.GetRawText(),
            StringComparison.Ordinal);
    }

    private static ExceptionMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next, NullLogger<ExceptionMiddleware>.Instance);

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
