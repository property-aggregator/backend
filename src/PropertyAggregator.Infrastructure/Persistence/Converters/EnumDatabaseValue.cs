namespace PropertyAggregator.Infrastructure.Persistence.Converters;

public static class EnumDatabaseValue
{
	public static string For<TEnum>(TEnum value) where TEnum : struct, Enum =>
		value.ToString().ToLowerInvariant();
}
