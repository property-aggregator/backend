namespace PropertyAggregator.Application.Offers.RequestOfferExpirationChecks;

public sealed record RequestOfferExpirationChecksCommand(int BatchSize, TimeSpan StaleAfter, TimeSpan RetryAfter);
