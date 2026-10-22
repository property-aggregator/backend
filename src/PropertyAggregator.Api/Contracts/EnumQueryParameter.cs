namespace PropertyAggregator.Api.Contracts;

public readonly record struct EnumQueryParameter<TEnum>(TEnum Value) where TEnum : struct, Enum
{
	public static bool TryParse(string? text, out EnumQueryParameter<TEnum> result)
	{
		if (Enum.TryParse(text, ignoreCase: true, out TEnum value) && Enum.IsDefined(value))
		{
			result = new EnumQueryParameter<TEnum>(value);
			return true;
		}

		result = default;
		return false;
	}
}
