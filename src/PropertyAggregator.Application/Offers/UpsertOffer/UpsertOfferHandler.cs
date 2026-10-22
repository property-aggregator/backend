using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Messaging.Contracts;
using PropertyAggregator.Domain.Models;
using PropertyAggregator.Domain.ValueObjects;

namespace PropertyAggregator.Application.Offers.UpsertOffer;

public sealed class UpsertOfferHandler(IPropertyRepository properties, IUnitOfWork unitOfWork) : ICommandHandler<UpsertOfferCommand>
{
	public async Task HandleAsync(UpsertOfferCommand command, CancellationToken cancellationToken)
	{
		OfferMessage offerMessage = command.Offer;

		Property? property = await properties.GetBySourceAsync(offerMessage.Portal, offerMessage.SourceId, cancellationToken);

		PropertyDetails details = new(
			SourceUrl: offerMessage.SourceUrl,
			Title: offerMessage.Title,
			MarketType: offerMessage.MarketType,
			Price: offerMessage.Price,
			AreaM2: offerMessage.AreaM2,
			Rooms: offerMessage.Rooms,
			Floor: offerMessage.Floor,
			SellerType: offerMessage.SellerType,
			Province: offerMessage.Province,
			City: offerMessage.City,
			District: offerMessage.District,
			Street: offerMessage.Street,
			MainImageUrl: offerMessage.ImageUrl);

		if (property is null)
		{
			property = new Property(
				sourcePortal: offerMessage.Portal,
				sourceId: offerMessage.SourceId,
				propertyType: offerMessage.PropertyType,
				transactionType: offerMessage.TransactionType,
				details: details,
				observedAt: offerMessage.ScrapedAt);

			properties.Add(property);
		}
		else
			property.ApplyObservation(details, offerMessage.ScrapedAt);

		await unitOfWork.SaveChangesAsync(cancellationToken);
	}
}
