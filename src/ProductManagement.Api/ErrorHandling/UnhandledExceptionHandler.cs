using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ProductManagement.Api.ErrorHandling;

public sealed partial class UnhandledExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		LogUnhandledException(logger, httpContext.Request.Method, httpContext.Request.Path, exception);

		httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
		return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = httpContext,
			ProblemDetails = new ProblemDetails
			{
				Status = StatusCodes.Status500InternalServerError,
				Title = "An unexpected error occurred.",
			},
		});
	}

	[LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception processing {Method} {Path}")]
	private static partial void LogUnhandledException(ILogger logger, string method, PathString path, Exception exception);
}