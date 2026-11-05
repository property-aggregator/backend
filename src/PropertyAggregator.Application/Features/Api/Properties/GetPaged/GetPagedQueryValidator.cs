using FluentValidation;
using PropertyAggregator.Application.Common.Validation;

namespace PropertyAggregator.Application.Features.Api.Properties.GetPaged;

public sealed class GetPagedQueryValidator : AbstractValidator<GetPagedQuery>
{
	private const int MaxPage = int.MaxValue / GetPagedQuery.MaxPageSize;

	public GetPagedQueryValidator()
	{
		RuleFor(query => query.LocationIds!.Length)
			.LessThanOrEqualTo(GetPagedQuery.MaxLocationIds)
			.When(query => query.LocationIds is not null)
			.OverridePropertyName(nameof(GetPagedQuery.LocationIds))
			.WithErrorCode(ValidationCodes.OutOfRange);

		RuleFor(query => query.PropertyType).IsInEnum().WithErrorCode(ValidationCodes.InvalidValue);
		RuleFor(query => query.TransactionType).IsInEnum().WithErrorCode(ValidationCodes.InvalidValue);
		RuleFor(query => query.MarketType).IsInEnum().WithErrorCode(ValidationCodes.InvalidValue);
		RuleFor(query => query.Sort).IsInEnum().WithErrorCode(ValidationCodes.InvalidValue);

		RuleFor(query => query.MinPrice).NonNegative();
		RuleFor(query => query.MaxPrice).NonNegative();
		RuleFor(query => query.MinAreaM2).NonNegative();
		RuleFor(query => query.MaxAreaM2).NonNegative();
		RuleFor(query => query.MinRooms).NonNegative();
		RuleFor(query => query.MaxRooms).NonNegative();

		RuleFor(query => query.MaxPrice)
			.GreaterThanOrEqualTo(query => query.MinPrice)
			.When(query => query.MinPrice is not null && query.MaxPrice is not null)
			.WithErrorCode(ValidationCodes.MaxLessThanMin);
		RuleFor(query => query.MaxAreaM2)
			.GreaterThanOrEqualTo(query => query.MinAreaM2)
			.When(query => query.MinAreaM2 is not null && query.MaxAreaM2 is not null)
			.WithErrorCode(ValidationCodes.MaxLessThanMin);
		RuleFor(query => query.MaxRooms)
			.GreaterThanOrEqualTo(query => query.MinRooms)
			.When(query => query.MinRooms is not null && query.MaxRooms is not null)
			.WithErrorCode(ValidationCodes.MaxLessThanMin);

		RuleFor(query => query.Page)
			.InclusiveBetween(1, MaxPage)
			.WithErrorCode(ValidationCodes.OutOfRange);
		RuleFor(query => query.PageSize)
			.InclusiveBetween(1, GetPagedQuery.MaxPageSize)
			.WithErrorCode(ValidationCodes.OutOfRange);
	}
}
