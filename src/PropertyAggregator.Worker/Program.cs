using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Geocoding.GeocodePendingProperties;
using PropertyAggregator.Application.Offers.UpsertOffer;
using PropertyAggregator.Infrastructure;
using PropertyAggregator.Worker.Consumers;
using PropertyAggregator.Worker.Jobs;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddPersistence();
builder.Services.AddGeocoding();
builder.Services.AddScoped<ICommandHandler<UpsertOfferCommand>, UpsertOfferHandler>();
builder.Services.AddScoped<ICommandHandler<GeocodePendingPropertiesCommand>, GeocodePendingPropertiesHandler>();
builder.Services.AddHostedService<OfferConsumer>();
builder.Services.AddHostedService<GeocodingJob>();

IHost host = builder.Build();
host.Run();
