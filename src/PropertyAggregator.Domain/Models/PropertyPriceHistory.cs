namespace PropertyAggregator.Domain.Models;

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
