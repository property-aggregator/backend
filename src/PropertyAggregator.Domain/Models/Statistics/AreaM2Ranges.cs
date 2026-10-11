namespace PropertyAggregator.Domain.Models.Statistics;

public static class AreaM2Ranges
{
	public const string All = "all";

	public static string For(decimal areaM2) => areaM2 switch
	{
		< 30 => "0-30",
		< 50 => "30-50",
		< 70 => "50-70",
		< 100 => "70-100",
		_ => "100+"
	};
}
