using System.Text.Json;
using System.Text.Json.Serialization;
using PropertyAggregator.Api.Security;
using PropertyAggregator.Api.Endpoints;
using PropertyAggregator.Infrastructure;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi(options => options.AddBearerSecurity());
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddPersistence();
builder.Services.AddConsentVersions();
builder.Services.AddJwtAuthentication();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
builder.Services.AddApiHandlers();

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapPropertyEndpoints();
app.MapUserEndpoints();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
	app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.Run();
