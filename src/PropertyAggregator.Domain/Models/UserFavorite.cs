namespace PropertyAggregator.Domain.Models;

public sealed class UserFavorite
{
	internal UserFavorite(Guid userId, Guid propertyId, DateTime createdAt)
	{
		UserId = userId;
		PropertyId = propertyId;
		CreatedAt = createdAt;
	}

	private UserFavorite()
	{
	}

	public Guid UserId { get; private set; }
	public Guid PropertyId { get; private set; }
	public DateTime CreatedAt { get; private set; }
}
