using PropertyAggregator.Application.Features.Api.Users.GetCurrentUser;

namespace PropertyAggregator.Api.Contracts.Users;

public sealed record CurrentUserResponse(
	Guid Id,
	bool TermsAccepted,
	string RequiredTermsVersion,
	bool MarketingConsent)
{
	public static CurrentUserResponse From(CurrentUserResult result) =>
		new(result.Id, result.TermsAccepted, result.RequiredTermsVersion, result.MarketingConsent);
}
