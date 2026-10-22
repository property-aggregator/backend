using PropertyAggregator.Application.Messaging.Contracts;

namespace PropertyAggregator.Application.Features.Worker.Offers.UpsertOffer;

public sealed record UpsertOfferCommand(OfferMessage Offer);
