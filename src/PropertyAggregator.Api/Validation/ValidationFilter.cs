using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;

namespace PropertyAggregator.Api.Validation;

public sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
	where TRequest : class
{
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
	{
		TRequest? request = context.Arguments.OfType<TRequest>().FirstOrDefault();

		if (request is null)
			return await next(context);

		ValidationResult result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

		if (result.IsValid)
			return await next(context);

		Dictionary<string, string[]> errors = result.Errors
			.GroupBy(failure => ToCamelCase(failure.PropertyName))
			.ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorCode).Distinct().ToArray());

		return TypedResults.ValidationProblem(errors);
	}

	private static string ToCamelCase(string propertyName) =>
		string.Join('.', propertyName.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
