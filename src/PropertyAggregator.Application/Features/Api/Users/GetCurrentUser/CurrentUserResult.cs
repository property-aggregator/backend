namespace PropertyAggregator.Application.Features.Api.Users.GetCurrentUser;

public sealed record CurrentUserResult(
	Guid Id,
	bool TermsAccepted,
	string RequiredTermsVersion,
	bool MarketingConsent,
	int FavoritesLimit);
