using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;
using PropertyAggregator.Infrastructure.Services;

namespace PropertyAggregator.Infrastructure.Health;

public sealed class DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = new())
	{
		try
		{
			string connectionString = EnvironmentService.SqlConnectionString;

			await using NpgsqlConnection connection = new(connectionString);
			await connection.OpenAsync(cancellationToken);

			await using NpgsqlCommand command = new("SELECT 1", connection);
			await command.ExecuteScalarAsync(cancellationToken);

			return HealthCheckResult.Healthy();
		}
		catch (Exception e)
		{
			logger.LogError(e, "Database health check failed");

			return HealthCheckResult.Unhealthy();
		}
	}
}
