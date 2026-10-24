namespace PropertyAggregator.Api.Errors;

public static class ErrorCodes
{
	public const string BadRequest = "bad_request";
	public const string Unauthorized = "unauthorized";
	public const string Forbidden = "forbidden";
	public const string NotFound = "not_found";
	public const string Conflict = "conflict";
	public const string InternalError = "internal_error";
	public const string ValidationFailed = "validation_failed";
	public const string ServiceUnavailable = "service_unavailable";

	public const string PropertyNotFound = "property_not_found";
	public const string FavoritesLimitReached = "favorites_limit_reached";
	public const string TermsMustBeAccepted = "terms_must_be_accepted";
	public const string TermsNotAccepted = "terms_not_accepted";
}
