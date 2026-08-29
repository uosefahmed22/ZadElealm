using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZadElealm.Apis.Middlwares;

namespace ZadElealm.UnitTests.Middleware;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ReturnsSafeProblemDetailsWithTraceId()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "test-trace-id"
        };
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("sensitive exception details"),
            CancellationToken.None);

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = Assert.IsType<ProblemDetails>(
            JsonSerializer.Deserialize<ProblemDetails>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("حدث خطأ غير متوقع.", problemDetails.Title);
        Assert.Equal("يرجى المحاولة لاحقاً.", problemDetails.Detail);
        Assert.Equal("/api/test", problemDetails.Instance);
        Assert.True(problemDetails.Extensions.TryGetValue("traceId", out var traceId));
        Assert.Equal("test-trace-id", traceId?.ToString());
        Assert.DoesNotContain("sensitive exception details", responseBody);
    }
}
