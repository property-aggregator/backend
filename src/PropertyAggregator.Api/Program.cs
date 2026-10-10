using Microsoft.Extensions.Diagnostics.HealthChecks;
using PropertyAggregator.Infrastructure.Health;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
	.AddCheck<DatabaseHealthCheck>("custom-sql", HealthStatus.Unhealthy);

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
	app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.Run();
