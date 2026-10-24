using Microsoft.AspNetCore.Http.HttpResults;

namespace PropertyAggregator.Api.Errors;

internal static class ApiProblems
{
	public static ProblemHttpResult PropertyNotFound() =>
		Create(StatusCodes.Status404NotFound, ErrorCodes.PropertyNotFound, "Property not found");

	public static ProblemHttpResult FavoritesLimitReached(int limit) =>
		Create(
			StatusCodes.Status409Conflict,
			ErrorCodes.FavoritesLimitReached,
			"Favorites limit reached",
			new Dictionary<string, object?> { ["limit"] = limit });

	public static ProblemHttpResult TermsMustBeAccepted() =>
		Create(StatusCodes.Status400BadRequest, ErrorCodes.TermsMustBeAccepted, "The terms must be accepted");

	private static ProblemHttpResult Create(
		int statusCode,
		string code,
		string title,
		Dictionary<string, object?>? extensions = null)
	{
		Dictionary<string, object?> all = new() { ["code"] = code };

		if (extensions is not null)
			foreach ((string key, object? value) in extensions)
				all[key] = value;

		return TypedResults.Problem(title: title, statusCode: statusCode, extensions: all);
	}
}
