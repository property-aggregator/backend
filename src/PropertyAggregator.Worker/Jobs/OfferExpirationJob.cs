using PropertyAggregator.Application.Features.Worker.Offers.RequestOfferExpirationChecks;

namespace PropertyAggregator.Worker.Jobs;

public sealed class OfferExpirationJob(IServiceScopeFactory scopeFactory, ILogger<OfferExpirationJob> logger)
	: PeriodicCommandJob<RequestOfferExpirationChecksCommand>(scopeFactory, logger, TimeSpan.FromMinutes(1))
{
	private const int BatchSize = 20;
	private static readonly TimeSpan StaleAfter = TimeSpan.FromHours(28);
	private static readonly TimeSpan RetryAfter = TimeSpan.FromHours(6);

	protected override RequestOfferExpirationChecksCommand CreateCommand() => new(BatchSize, StaleAfter, RetryAfter);
}
