namespace PropertyAggregator.Application.Features.Api.Users.AcceptConsents;

public sealed record AcceptConsentsCommand(string ExternalId, bool MarketingConsent);
