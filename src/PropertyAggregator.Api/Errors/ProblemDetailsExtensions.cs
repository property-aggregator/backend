namespace PropertyAggregator.Api.Errors;

public static class ProblemDetailsExtensions
{
	public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
	{
		services.AddExceptionHandler<ApiExceptionHandler>();

		return services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
		{
			string? code = context.ProblemDetails switch
			{
				HttpValidationProblemDetails => ErrorCodes.ValidationFailed,
				{ Status: StatusCodes.Status400BadRequest } => ErrorCodes.BadRequest,
				{ Status: StatusCodes.Status401Unauthorized } => ErrorCodes.Unauthorized,
				{ Status: StatusCodes.Status403Forbidden } => ErrorCodes.Forbidden,
				{ Status: StatusCodes.Status404NotFound } => ErrorCodes.NotFound,
				{ Status: StatusCodes.Status409Conflict } => ErrorCodes.Conflict,
				{ Status: StatusCodes.Status503ServiceUnavailable } => ErrorCodes.ServiceUnavailable,
				{ Status: >= StatusCodes.Status500InternalServerError } => ErrorCodes.InternalError,
				{ Status: >= StatusCodes.Status400BadRequest and < StatusCodes.Status500InternalServerError } => ErrorCodes.BadRequest,
				_ => null
			};

			if (code is not null)
				context.ProblemDetails.Extensions.TryAdd("code", code);
		});
	}
}
