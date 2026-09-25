using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ProductManagement.Api.ErrorHandling;

public sealed class BadHttpRequestExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badHttpRequestException)
        {
            return false;
        }

        httpContext.Response.StatusCode = badHttpRequestException.StatusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = badHttpRequestException,
            ProblemDetails = new ProblemDetails
            {
                Status = badHttpRequestException.StatusCode,
                Title = "Bad request",
                Detail = badHttpRequestException.Message,
            },
        });
    }
}
