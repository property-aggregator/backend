namespace PropertyAggregator.Api.Validation;

public static class ValidationExtensions
{
	public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
		where TRequest : class =>
		builder.AddEndpointFilter<ValidationFilter<TRequest>>().ProducesValidationProblem();
}
