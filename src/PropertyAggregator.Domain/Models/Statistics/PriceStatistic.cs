using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Domain.Models.Statistics;

public sealed class PriceStatistic
{
	public Guid Id { get; init; }
	public string? Province { get; init; }
	public string? City { get; init; }
	public string? District { get; init; }
	public required PropertyType PropertyType { get; init; }
	public required TransactionType TransactionType { get; init; }
	public required MarketType MarketType { get; init; }
	public required string AreaM2Range { get; init; }
	public required decimal AvgPricePerM2 { get; init; }
	public required decimal MedianPricePerM2 { get; init; }
	public required int SampleSize { get; init; }
	public required DateTime CalculatedAt { get; init; }
}
