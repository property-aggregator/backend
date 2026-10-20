using PropertyAggregator.Application.Messaging.Contracts;

namespace PropertyAggregator.Application.Offers.UpsertOffer;

public sealed record UpsertOfferCommand(OfferMessage Offer);
