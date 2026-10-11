using PropertyAggregator.Infrastructure;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddPersistence();

IHost host = builder.Build();
host.Run();
