using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Domain.Models;

public sealed class PriceStatistic
{
	public Guid Id { get; init; }
	public required string City { get; init; }
	public string? District { get; init; }
	public required PropertyType PropertyType { get; init; }
	public required TransactionType TransactionType { get; init; }
	public required MarketType MarketType { get; init; }
	public required AreaM2Range AreaM2Range { get; init; }
	public required decimal AvgPricePerM2 { get; init; }
	public required decimal MedianPricePerM2 { get; init; }
	public required int SampleSize { get; init; }
	public required DateOnly CalculatedOn { get; init; }

	public static IReadOnlyList<(decimal UpperBound, AreaM2Range Range)> AreaRangeBounds { get; } =
	[
		(30, AreaM2Range.Under30),
		(50, AreaM2Range.From30To50),
		(70, AreaM2Range.From50To70),
		(100, AreaM2Range.From70To100)
	];

	public static AreaM2Range RangeFor(decimal areaM2)
	{
		foreach ((decimal upperBound, AreaM2Range range) in AreaRangeBounds)
		{
			if (areaM2 < upperBound)
				return range;
		}

		return AreaM2Range.Over100;
	}
}
