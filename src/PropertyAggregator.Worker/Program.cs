using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Geocoding.GeocodePendingProperties;
using PropertyAggregator.Application.Offers.UpsertOffer;
using PropertyAggregator.Application.Offers.ApplyOfferExpirationResponse;
using PropertyAggregator.Application.Offers.RequestOfferExpirationChecks;
using PropertyAggregator.Infrastructure;
using PropertyAggregator.Worker.Consumers;
using PropertyAggregator.Worker.Jobs;
using PropertyAggregator.Worker.Messaging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddPersistence();
builder.Services.AddGeocoding();
builder.Services.AddScoped<ICommandHandler<UpsertOfferCommand>, UpsertOfferHandler>();
builder.Services.AddScoped<ICommandHandler<GeocodePendingPropertiesCommand>, GeocodePendingPropertiesHandler>();
builder.Services.AddScoped<ICommandHandler<RequestOfferExpirationChecksCommand>, RequestOfferExpirationChecksHandler>();
builder.Services.AddScoped<ICommandHandler<ApplyOfferExpirationResponseCommand>, ApplyOfferExpirationResponseHandler>();
builder.Services.AddSingleton<IOfferExpirationCheckPublisher, RabbitMqOfferExpirationCheckPublisher>();
builder.Services.AddHostedService<OfferConsumer>();
builder.Services.AddHostedService<ExpirationResponseConsumer>();
builder.Services.AddHostedService<GeocodingJob>();
builder.Services.AddHostedService<OfferExpirationJob>();

IHost host = builder.Build();
host.Run();
