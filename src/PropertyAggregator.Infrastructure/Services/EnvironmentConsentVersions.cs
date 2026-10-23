using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Infrastructure.Services;

internal sealed class EnvironmentConsentVersions : IConsentVersions
{
	private const int MaxVersionLength = 20;

	public EnvironmentConsentVersions()
	{
		Terms = Validate(EnvironmentService.TermsVersion, "TERMS_VERSION");
		Marketing = Validate(EnvironmentService.MarketingConsentVersion, "MARKETING_CONSENT_VERSION");
	}

	public string Terms { get; }

	public string Marketing { get; }

	private static string Validate(string version, string variable) =>
		version.Length <= MaxVersionLength
			? version
			: throw new InvalidOperationException($"Environment variable '{variable}' must be at most {MaxVersionLength} characters");
}
