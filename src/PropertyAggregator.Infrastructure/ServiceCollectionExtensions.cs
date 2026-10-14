using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Infrastructure.Persistence;
using PropertyAggregator.Infrastructure.Services;

namespace PropertyAggregator.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPersistence(this IServiceCollection services)
	{
		services.AddDbContext<AppDbContext>(options => options
			.UseNpgsql(EnvironmentService.SqlConnectionString, npgsql => npgsql.UseNetTopologySuite())
			.UseCamelCaseNamingConvention());

		services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

		services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

		return services;
	}
}
