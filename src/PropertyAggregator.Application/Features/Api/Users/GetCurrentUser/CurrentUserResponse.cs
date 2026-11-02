namespace PropertyAggregator.Application.Features.Api.Users.GetCurrentUser;

public sealed record CurrentUserResponse(
	Guid Id,
	bool TermsAccepted,
	string RequiredTermsVersion,
	bool MarketingConsent,
	int FavoritesLimit);
