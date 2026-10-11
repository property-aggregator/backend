namespace PropertyAggregator.Domain.Models.Properties;

/// <summary>Price of an offer at a point in time. Created only by <see cref="Property"/>.</summary>
public sealed class PropertyPriceHistory
{
	internal PropertyPriceHistory(Guid propertyId, decimal? price, DateTime createdAt)
	{
		Id = Guid.CreateVersion7();
		PropertyId = propertyId;
		Price = price;
		CreatedAt = createdAt;
	}

	private PropertyPriceHistory()
	{
	}

	public Guid Id { get; private set; }
	public Guid PropertyId { get; private set; }
	public decimal? Price { get; private set; }
	public DateTime CreatedAt { get; private set; }
}
