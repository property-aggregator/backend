using System.Data.Common;
using Microsoft.AspNetCore.Diagnostics;
using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Api.Errors;

internal sealed class ApiExceptionHandler(
	IProblemDetailsService problemDetailsService,
	ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext context,
		Exception exception,
		CancellationToken cancellationToken)
	{
		int? status = InnerChain(exception).Select(MapToStatus).FirstOrDefault(s => s is not null);

		if (status is null)
			return false;

		logger.Log(
			status.Value >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning,
			exception,
			"Handled exception as {StatusCode} for {Method} {Path}",
			status.Value,
			context.Request.Method,
			context.Request.Path);

		context.Response.StatusCode = status.Value;

		return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = context,
			Exception = exception,
			ProblemDetails = { Status = status }
		});
	}

	private static int? MapToStatus(Exception exception) => exception switch
	{
		BadHttpRequestException badRequest => badRequest.StatusCode,
		UniqueConstraintViolationException => StatusCodes.Status409Conflict,
		DbException { IsTransient: true } => StatusCodes.Status503ServiceUnavailable,
		_ => null
	};

	private static IEnumerable<Exception> InnerChain(Exception exception)
	{
		for (Exception? current = exception; current is not null; current = current.InnerException)
			yield return current;
	}
}
