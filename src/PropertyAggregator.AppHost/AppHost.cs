IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PropertyAggregator_Api>("api")
	.WithHttpHealthCheck("/health");
builder.AddProject<Projects.PropertyAggregator_Worker>("worker");

builder.Build().Run();
