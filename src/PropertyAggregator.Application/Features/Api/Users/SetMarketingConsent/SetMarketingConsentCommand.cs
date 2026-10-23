namespace PropertyAggregator.Application.Features.Api.Users.SetMarketingConsent;

public sealed record SetMarketingConsentCommand(string ExternalId, bool Granted);
