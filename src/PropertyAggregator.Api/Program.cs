using System.Text.Json;
using System.Text.Json.Serialization;
using PropertyAggregator.Api.Endpoints;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Common;
using PropertyAggregator.Application.Properties;
using PropertyAggregator.Application.Properties.GetById;
using PropertyAggregator.Application.Properties.GetPaged;
using PropertyAggregator.Infrastructure;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddPersistence();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
builder.Services.AddScoped<IQueryHandler<GetByIdQuery, PropertyResult?>, GetByIdHandler>();
builder.Services.AddScoped<IQueryHandler<GetPagedQuery, PagedResult<PropertyResult>>, GetPagedHandler>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapPropertyEndpoints();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
	app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.Run();
