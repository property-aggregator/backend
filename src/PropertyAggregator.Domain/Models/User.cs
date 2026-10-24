using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Domain.Models;

public sealed class User
{
	public const int FavoritesLimit = 10;

	private readonly List<UserConsent> _consents = [];
	private readonly List<UserFavorite> _favorites = [];

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
	public IReadOnlyCollection<UserFavorite> Favorites => _favorites;

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

	public AddFavoriteResult AddFavorite(Guid propertyId, DateTime addedAt)
	{
		if (_favorites.Any(favorite => favorite.PropertyId == propertyId))
			return AddFavoriteResult.AlreadyFavorite;

		if (_favorites.Count >= FavoritesLimit)
			return AddFavoriteResult.LimitReached;

		_favorites.Add(new UserFavorite(Id, propertyId, addedAt));

		return AddFavoriteResult.Added;
	}

	public void RemoveFavorite(Guid propertyId) =>
		_favorites.RemoveAll(favorite => favorite.PropertyId == propertyId);

	private UserConsent? LatestConsent(ConsentType type) =>
		_consents.Where(consent => consent.Type == type).MaxBy(consent => consent.CreatedAt);
}
