using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PropertyAggregator.Infrastructure.Persistence.Converters;

public sealed class LowercaseEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
	value => value.ToString().ToLowerInvariant(),
	value => Enum.Parse<TEnum>(value, true))
	where TEnum : struct, Enum;
