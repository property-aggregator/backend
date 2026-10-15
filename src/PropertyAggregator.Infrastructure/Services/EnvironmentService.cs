namespace PropertyAggregator.Infrastructure.Services;

public static class EnvironmentService
{
	public static string SqlConnectionString => GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

	public static string RabbitMqHost => GetEnvironmentVariable("RABBITMQ_HOST");
	public static int RabbitMqPort => GetIntEnvironmentVariable("RABBITMQ_PORT");
	public static string RabbitMqUsername => GetEnvironmentVariable("RABBITMQ_USERNAME");
	public static string RabbitMqPassword => GetEnvironmentVariable("RABBITMQ_PASSWORD");
	public static string RabbitMqQueue => GetEnvironmentVariable("RABBITMQ_QUEUE");
	public static string RabbitMqExchange => GetEnvironmentVariable("RABBITMQ_EXCHANGE");

	public static string NominatimBaseUrl => GetEnvironmentVariable("NOMINATIM_BASE_URL");
	public static string NominatimUserAgent => GetEnvironmentVariable("NOMINATIM_USER_AGENT");

	private static string GetEnvironmentVariable(string variable)
	{
		string? value = Environment.GetEnvironmentVariable(variable);

		if (string.IsNullOrWhiteSpace(value))
		{
			string errorMessage = $"Environment variable '{variable}' not found or is empty";
			throw new InvalidOperationException(errorMessage);
		}

		return value;
	}

	private static int GetIntEnvironmentVariable(string variable)
	{
		string value = GetEnvironmentVariable(variable);

		if (!int.TryParse(value, out int result) || result <= 0)
		{
			string errorMessage = $"Environment variable '{variable}' must be a positive integer";
			throw new InvalidOperationException(errorMessage);
		}

		return result;
	}
}
