using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Infrastructure.Geocoding;
using PropertyAggregator.Infrastructure.Http;
using PropertyAggregator.Infrastructure.Persistence;
using PropertyAggregator.Infrastructure.Persistence.Queries;
using PropertyAggregator.Infrastructure.Persistence.Repositories;
using PropertyAggregator.Infrastructure.Services;

namespace PropertyAggregator.Infrastructure;

public static class DependencyInjection
{
	// Nominatim usage policy allows at most one request per second; the margin absorbs network jitter.
	private static readonly TimeSpan MinNominatimRequestInterval = TimeSpan.FromMilliseconds(1100);

	public static IServiceCollection AddApiHandlers(this IServiceCollection services) =>
		services.AddHandlersFrom("PropertyAggregator.Application.Features.Api");

	public static IServiceCollection AddWorkerHandlers(this IServiceCollection services) =>
		services.AddHandlersFrom("PropertyAggregator.Application.Features.Worker");

	private static IServiceCollection AddHandlersFrom(this IServiceCollection services, string handlersNamespace) =>
		services.Scan(scan => scan
			.FromAssemblies(typeof(ICommandHandler<>).Assembly)
			.AddClasses(classes => classes.InNamespaces(handlersNamespace).AssignableTo(typeof(IQueryHandler<,>)))
			.AsImplementedInterfaces()
			.WithScopedLifetime()
			.AddClasses(classes => classes.InNamespaces(handlersNamespace).AssignableTo(typeof(ICommandHandler<>)))
			.AsImplementedInterfaces()
			.WithScopedLifetime());

	public static IServiceCollection AddPersistence(this IServiceCollection services)
	{
		services.AddDbContext<AppDbContext>(options => options
			.UseNpgsql(EnvironmentService.SqlConnectionString, npgsql => npgsql.UseNetTopologySuite())
			.UseCamelCaseNamingConvention());

		services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
		services.AddScoped<IPropertyRepository, PropertyRepository>();
		services.AddScoped<IPropertyQueries, PropertyQueries>();

		services.AddScoped<IPriceStatisticsRepository, PriceStatisticsRepository>();

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
