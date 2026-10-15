using PropertyAggregator.Application.Offers.Contracts;

namespace PropertyAggregator.Application.Offers.UpsertOffer;

public sealed record UpsertOfferCommand(OfferMessage Offer);
