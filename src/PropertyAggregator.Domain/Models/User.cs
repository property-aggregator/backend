using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Domain.Models;

public sealed class User
{
	private readonly List<UserConsent> _consents = [];

	public User(string externalId, DateTime createdAt)
	{
		Id = Guid.CreateVersion7();
		ExternalId = externalId;
		CreatedAt = createdAt;
	}

	private User()
	{
	}

	public Guid Id { get; private set; }
	public string ExternalId { get; private set; } = null!;
	public DateTime CreatedAt { get; private set; }

	public IReadOnlyCollection<UserConsent> Consents => _consents;

	public bool HasMarketingConsent => LatestConsent(ConsentType.Marketing)?.Granted == true;

	public bool HasAcceptedTerms(string version) =>
		LatestConsent(ConsentType.Terms) is { Granted: true } consent && consent.Version == version;

	public void AcceptTerms(string version, DateTime acceptedAt)
	{
		if (HasAcceptedTerms(version))
			return;

		_consents.Add(new UserConsent(Id, ConsentType.Terms, version, granted: true, acceptedAt));
	}

	public void SetMarketingConsent(string version, bool granted, DateTime changedAt)
	{
		if (HasMarketingConsent == granted)
			return;

		_consents.Add(new UserConsent(Id, ConsentType.Marketing, version, granted, changedAt));
	}

	private UserConsent? LatestConsent(ConsentType type) =>
		_consents.Where(consent => consent.Type == type).MaxBy(consent => consent.CreatedAt);
}
