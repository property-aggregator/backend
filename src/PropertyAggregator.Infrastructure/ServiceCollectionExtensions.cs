using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Infrastructure.Geocoding;
using PropertyAggregator.Infrastructure.Http;
using PropertyAggregator.Infrastructure.Persistence;
using PropertyAggregator.Infrastructure.Services;

namespace PropertyAggregator.Infrastructure;

public static class ServiceCollectionExtensions
{
	// Nominatim usage policy allows at most one request per second; the margin absorbs network jitter.
	private static readonly TimeSpan MinNominatimRequestInterval = TimeSpan.FromMilliseconds(1100);

	public static IServiceCollection AddPersistence(this IServiceCollection services)
	{
		services.AddDbContext<AppDbContext>(options => options
			.UseNpgsql(EnvironmentService.SqlConnectionString, npgsql => npgsql.UseNetTopologySuite())
			.UseCamelCaseNamingConvention());

		services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

		services.AddScoped<IPriceStatisticsWriter, PriceStatisticsWriter>();

		services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

		return services;
	}

	public static IServiceCollection AddGeocoding(this IServiceCollection services)
	{
		string baseUrl = EnvironmentService.NominatimBaseUrl;
		string userAgent = EnvironmentService.NominatimUserAgent;

		services.AddSingleton(new RequestThrottle(MinNominatimRequestInterval));
		services.AddTransient<ThrottlingHandler>();

		IHttpClientBuilder nominatimClient = services.AddHttpClient<NominatimGeocoder>(client =>
		{
			client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
			client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
		});

		nominatimClient.AddStandardResilienceHandler();
		nominatimClient.AddHttpMessageHandler<ThrottlingHandler>();

		services.AddScoped<IGeocoder, CachedGeocoder>();

		return services;
	}
}
