namespace PropertyAggregator.Api.Errors;

public static class ProblemDetailsExtensions
{
	public static IServiceCollection AddApiProblemDetails(this IServiceCollection services) =>
		services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
		{
			string? code = context.ProblemDetails.Status switch
			{
				StatusCodes.Status400BadRequest => ErrorCodes.BadRequest,
				StatusCodes.Status403Forbidden => ErrorCodes.Forbidden,
				StatusCodes.Status404NotFound => ErrorCodes.NotFound,
				StatusCodes.Status409Conflict => ErrorCodes.Conflict,
				>= StatusCodes.Status500InternalServerError => ErrorCodes.InternalError,
				_ => null
			};

			if (code is not null)
				context.ProblemDetails.Extensions.TryAdd("code", code);
		});
}
