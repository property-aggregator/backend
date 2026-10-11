using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Domain.Models.Properties;

public sealed class Property
{
	private readonly List<PropertyPriceHistory> _propertyPriceHistories = [];

	public Property(
		string sourcePortal,
		string sourceId,
		PropertyType propertyType,
		TransactionType transactionType,
		MarketType marketType,
		decimal? price,
		DateTime now)
	{
		Id = Guid.CreateVersion7();
		SourcePortal = sourcePortal;
		SourceId = sourceId;
		PropertyType = propertyType;
		TransactionType = transactionType;
		MarketType = marketType;
		Price = price;
		FirstSeenAt = now;
		LastSeenAt = now;
		UpdatedAt = now;
		_propertyPriceHistories.Add(new PropertyPriceHistory(Id, price, now));
	}

	private Property()
	{
	}

	public Guid Id { get; private set; }
	public string SourcePortal { get; private set; } = null!;
	public string SourceId { get; private set; } = null!;
	public required string SourceUrl { get; set; }
	public required string Title { get; set; }
	public decimal? Price { get; private set; }
	public decimal? PricePerM2 { get; set; }
	public decimal? AreaM2 { get; set; }
	public int? Rooms { get; set; }
	public string? Floor { get; set; }
	public PropertyType PropertyType { get; private set; }
	public TransactionType TransactionType { get; private set; }
	public MarketType MarketType { get; private set; }
	public SellerType? SellerType { get; set; }
	public string? Province { get; set; }
	public string? City { get; set; }
	public string? District { get; set; }
	public string? Street { get; set; }
	public GeoPoint? Location { get; set; }
	public string? MainImageUrl { get; set; }
	public bool IsDeal { get; set; }
	public decimal? DealDiscountPercent { get; set; }
	public DateTime FirstSeenAt { get; private set; }
	public DateTime LastSeenAt { get; set; }
	public DateTime? ArchivedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public IReadOnlyCollection<PropertyPriceHistory> PropertyPriceHistories => _propertyPriceHistories;

	public void SetPrice(decimal? price, DateTime now)
	{
		if (price == Price)
			return;

		Price = price;
		_propertyPriceHistories.Add(new PropertyPriceHistory(Id, price, now));
	}
}
