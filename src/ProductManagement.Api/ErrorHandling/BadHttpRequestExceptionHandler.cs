using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ProductManagement.Api.ErrorHandling;

public sealed partial class BadHttpRequestExceptionHandler(
    IProblemDetailsService problemDetailsService, ILogger<BadHttpRequestExceptionHandler> logger) : IExceptionHandler
{
    public const string Detail = "The request could not be read. Check that route and query values have the expected types and that the body is valid JSON.";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badHttpRequestException)
        {
            return false;
        }

        LogMalformedRequest(logger, badHttpRequestException);
        httpContext.Response.StatusCode = badHttpRequestException.StatusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = badHttpRequestException,
            ProblemDetails = new ProblemDetails
            {
                Status = badHttpRequestException.StatusCode,
                Title = "Bad request",
                Detail = Detail,
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Rejected a request that could not be bound")]
    private static partial void LogMalformedRequest(ILogger logger, Exception exception);
}
