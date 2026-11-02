namespace PropertyAggregator.Application.Features.Api.Users.AcceptConsents;

public sealed record AcceptConsentsCommand(bool AcceptTerms, bool MarketingConsent);
