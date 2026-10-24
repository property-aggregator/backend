using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PropertyAggregator.Api.Security;

public static class OpenApiExtensions
{
	private const string BearerSchemeName = "Bearer";

	public static OpenApiOptions AddBearerSecurity(this OpenApiOptions options)
	{
		options.AddDocumentTransformer((document, _, _) =>
		{
			document.Components ??= new OpenApiComponents();
			document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
			document.Components.SecuritySchemes[BearerSchemeName] = new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT"
			};

			return Task.CompletedTask;
		});

		options.AddOperationTransformer((operation, context, _) =>
		{
			IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;

			if (!metadata.OfType<IAuthorizeData>().Any() || metadata.OfType<IAllowAnonymous>().Any())
				return Task.CompletedTask;

			operation.Security ??= [];
			operation.Security.Add(new OpenApiSecurityRequirement
			{
				[new OpenApiSecuritySchemeReference(BearerSchemeName, context.Document)] = []
			});

			return Task.CompletedTask;
		});

		return options;
	}
}
