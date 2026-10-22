using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.ValueObjects;

namespace PropertyAggregator.Domain.Models;

public sealed class Property
{
	private readonly List<PropertyPriceHistory> _propertyPriceHistories = [];

	public Property(
		string sourcePortal,
		string sourceId,
		PropertyType propertyType,
		TransactionType transactionType,
		PropertyDetails details,
		DateTime observedAt)
	{
		Id = Guid.CreateVersion7();
		SourcePortal = sourcePortal;
		SourceId = sourceId;
		PropertyType = propertyType;
		TransactionType = transactionType;
		FirstSeenAt = observedAt;

		ApplyObservation(details, observedAt);
	}

	private Property()
	{
	}

	public Guid Id { get; private set; }
	public string SourcePortal { get; private set; } = null!;
	public string SourceId { get; private set; } = null!;
	public string SourceUrl { get; private set; } = null!;

	public PropertyType PropertyType { get; private set; }
	public TransactionType TransactionType { get; private set; }
	public MarketType MarketType { get; private set; }
	public SellerType? SellerType { get; private set; }

	public string Title { get; private set; } = null!;
	public decimal? Price { get; private set; }
	public decimal? AreaM2 { get; private set; }
	public decimal? PricePerM2 { get; private set; }
	public int? Rooms { get; private set; }
	public string? Floor { get; private set; }
	public string? MainImageUrl { get; private set; }

	public string? Province { get; private set; }
	public string? City { get; private set; }
	public string? District { get; private set; }
	public string? Street { get; private set; }
	public GeoPoint? Location { get; private set; }
	public LocationPrecision? LocationPrecision { get; private set; }
	public DateTime? GeocodedAt { get; private set; }

	public bool IsDeal { get; private set; }
	public decimal? DealDiscountPercent { get; private set; }

	public DateTime FirstSeenAt { get; private set; }
	public DateTime LastSeenAt { get; private set; }
	public DateTime UpdatedAt { get; private set; }
	public DateTime? ArchivedAt { get; private set; }
	public DateTime? ExpirationCheckRequestedAt { get; private set; }

	public IReadOnlyCollection<PropertyPriceHistory> PropertyPriceHistories => _propertyPriceHistories;

	private PropertyDetails CurrentDetails => new(
		SourceUrl, Title, MarketType, Price, AreaM2, Rooms, Floor, SellerType,
		Province, City, District, Street, MainImageUrl);

	private const decimal DealThresholdPercent = 15;

	public void ApplyObservation(PropertyDetails details, DateTime observedAt)
	{
		if (details != CurrentDetails)
		{
			ChangePrice(details.Price, observedAt);
			CopyDetails(details);
			RecalculatePricePerM2();
			UpdatedAt = observedAt;
		}

		if (ArchivedAt is not null)
		{
			ArchivedAt = null;
			UpdatedAt = observedAt;
		}

		MarkSeen(observedAt);
	}

	public void EvaluateDeal(decimal? medianPricePerM2)
	{
		decimal? discount = PricePerM2 is { } pricePerM2 && medianPricePerM2 is > 0
			? Math.Round((medianPricePerM2.Value - pricePerM2) / medianPricePerM2.Value * 100, 2,
				MidpointRounding.AwayFromZero)
			: null;

		IsDeal = discount >= DealThresholdPercent;
		DealDiscountPercent = IsDeal ? discount : null;
	}

	public void MarkSeen(DateTime seenAt) =>
		LastSeenAt = seenAt;

	public void MarkExpirationCheckRequested(DateTime requestedAt) =>
		ExpirationCheckRequestedAt = requestedAt;

	public void Archive(DateTime archivedAt)
	{
		if (ArchivedAt is not null)
			return;

		ArchivedAt = archivedAt;
		UpdatedAt = archivedAt;
	}

	public void SetLocation(GeoPoint location, LocationPrecision precision, DateTime geocodedAt)
	{
		Location = location;
		LocationPrecision = precision;
		GeocodedAt = geocodedAt;
	}

	public void MarkGeocodingFailed(DateTime geocodedAt)
	{
		Location = null;
		LocationPrecision = null;
		GeocodedAt = geocodedAt;
	}

	private void ChangePrice(decimal? price, DateTime changedAt)
	{
		if (price == Price)
			return;

		Price = price;
		_propertyPriceHistories.Add(new PropertyPriceHistory(Id, price, changedAt));
	}

	private void CopyDetails(PropertyDetails details)
	{
		SourceUrl = details.SourceUrl;
		MarketType = details.MarketType;
		SellerType = details.SellerType;

		Title = details.Title;
		AreaM2 = details.AreaM2;
		Rooms = details.Rooms;
		Floor = details.Floor;
		MainImageUrl = details.MainImageUrl;

		CopyAddress(details);
	}

	private void CopyAddress(PropertyDetails details)
	{
		bool addressChanged = Province != details.Province
							|| City != details.City
							|| District != details.District
							|| Street != details.Street;

		Province = details.Province;
		City = details.City;
		District = details.District;
		Street = details.Street;

		if (addressChanged)
			ResetLocation();
	}

	private void ResetLocation()
	{
		Location = null;
		LocationPrecision = null;
		GeocodedAt = null;
	}

	private void RecalculatePricePerM2() =>
		PricePerM2 = Price > 0 && AreaM2 > 0
			? Math.Round(Price.Value / AreaM2.Value, 2, MidpointRounding.AwayFromZero)
			: null;
}
