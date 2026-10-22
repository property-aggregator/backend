using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Infrastructure;
using PropertyAggregator.Worker.Consumers;
using PropertyAggregator.Worker.Jobs;
using PropertyAggregator.Infrastructure.Messaging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddPersistence();
builder.Services.AddGeocoding();
builder.Services.AddWorkerHandlers();
builder.Services.AddSingleton<IOfferExpirationCheckPublisher, RabbitMqOfferExpirationCheckPublisher>();
builder.Services.AddHostedService<OfferConsumer>();
builder.Services.AddHostedService<ExpirationResponseConsumer>();
builder.Services.AddHostedService<GeocodingJob>();
builder.Services.AddHostedService<OfferExpirationJob>();
builder.Services.AddHostedService<PriceStatisticsJob>();

IHost host = builder.Build();
host.Run();
