namespace PropertyAggregator.Application.Features.Worker.Offers.RequestOfferExpirationChecks;

public sealed record RequestOfferExpirationChecksCommand(int BatchSize, TimeSpan StaleAfter, TimeSpan RetryAfter);
