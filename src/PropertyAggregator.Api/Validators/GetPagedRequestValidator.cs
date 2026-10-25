using FluentValidation;
using PropertyAggregator.Api.Contracts.Properties;
using PropertyAggregator.Api.Validation;
using PropertyAggregator.Application.Features.Api.Properties.GetPaged;

namespace PropertyAggregator.Api.Validators;

public sealed class GetPagedRequestValidator : AbstractValidator<GetPagedRequest>
{
	public const int MaxLocationLength = 200;

	public const int MaxPage = int.MaxValue / GetPagedQuery.MaxPageSize;

	public GetPagedRequestValidator()
	{
		RuleFor(request => request.Location).MaximumLength(MaxLocationLength).WithErrorCode(ValidationCodes.TooLong);

		RuleFor(request => request.PropertyType).IsInEnum().WithErrorCode(ValidationCodes.InvalidValue);
		RuleFor(request => request.TransactionType).IsInEnum().WithErrorCode(ValidationCodes.InvalidValue);
		RuleFor(request => request.MarketType).IsInEnum().WithErrorCode(ValidationCodes.InvalidValue);
		RuleFor(request => request.Sort).IsInEnum().WithErrorCode(ValidationCodes.InvalidValue);

		RuleFor(request => request.MinPrice).NonNegative();
		RuleFor(request => request.MaxPrice).NonNegative();
		RuleFor(request => request.MinAreaM2).NonNegative();
		RuleFor(request => request.MaxAreaM2).NonNegative();
		RuleFor(request => request.MinRooms).NonNegative();
		RuleFor(request => request.MaxRooms).NonNegative();

		RuleFor(request => request.MaxPrice)
			.GreaterThanOrEqualTo(request => request.MinPrice)
			.When(request => request.MinPrice is not null && request.MaxPrice is not null)
			.WithErrorCode(ValidationCodes.MaxLessThanMin);
		RuleFor(request => request.MaxAreaM2)
			.GreaterThanOrEqualTo(request => request.MinAreaM2)
			.When(request => request.MinAreaM2 is not null && request.MaxAreaM2 is not null)
			.WithErrorCode(ValidationCodes.MaxLessThanMin);
		RuleFor(request => request.MaxRooms)
			.GreaterThanOrEqualTo(request => request.MinRooms)
			.When(request => request.MinRooms is not null && request.MaxRooms is not null)
			.WithErrorCode(ValidationCodes.MaxLessThanMin);

		RuleFor(request => request.Page)
			.InclusiveBetween(1, MaxPage)
			.WithErrorCode(ValidationCodes.OutOfRange);
		RuleFor(request => request.PageSize)
			.InclusiveBetween(1, GetPagedQuery.MaxPageSize)
			.WithErrorCode(ValidationCodes.OutOfRange);
	}
}
