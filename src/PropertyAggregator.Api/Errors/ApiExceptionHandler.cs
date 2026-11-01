using System.Data.Common;
using System.Text.Json;
using FluentValidation;
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
		if (exception is ValidationException validationException)
			return await WriteValidationProblemAsync(context, validationException);

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

	// Invalid input is an expected client error, so it is not logged.
	private async ValueTask<bool> WriteValidationProblemAsync(HttpContext context, ValidationException exception)
	{
		Dictionary<string, string[]> errors = exception.Errors
			.GroupBy(failure => ToCamelCase(failure.PropertyName))
			.ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorCode).Distinct().ToArray());

		context.Response.StatusCode = StatusCodes.Status400BadRequest;

		return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = context,
			ProblemDetails = new HttpValidationProblemDetails(errors) { Status = StatusCodes.Status400BadRequest }
		});
	}

	private static string ToCamelCase(string propertyName) =>
		string.Join('.', propertyName.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));

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
