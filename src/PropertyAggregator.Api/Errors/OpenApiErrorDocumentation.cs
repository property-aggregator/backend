using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using PropertyAggregator.Api.Security;
using PropertyAggregator.Api.Validation;

namespace PropertyAggregator.Api.Errors;

public static class OpenApiErrorDocumentation
{
	private const string ProblemJson = "application/problem+json";
	private const string ProblemDetailsSchemaId = "ProblemDetails";

	private static readonly string[] ErrorCodeValues = ConstantValues(typeof(ErrorCodes));
	private static readonly string[] ValidationCodeValues = ConstantValues(typeof(ValidationCodes));

	public static OpenApiOptions AddErrorDocumentation(this OpenApiOptions options)
	{
		options.AddSchemaTransformer((schema, context, _) =>
		{
			Type type = context.JsonTypeInfo.Type;

			if (type != typeof(ProblemDetails) && type != typeof(HttpValidationProblemDetails))
				return Task.CompletedTask;

			schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
			schema.Properties["code"] = new OpenApiSchema
			{
				Type = JsonSchemaType.String,
				Description = "Stable machine-readable error code. Clients translate this; title and detail are for developers.",
				Enum = ToJsonValues(ErrorCodeValues)
			};
			schema.Properties["traceId"] = new OpenApiSchema
			{
				Type = JsonSchemaType.String,
				Description = "Correlation id of the request; quote it when reporting a problem."
			};

			schema.Required ??= new HashSet<string>();
			schema.Required.Add("code");
			schema.Required.Add("traceId");

			if (type == typeof(HttpValidationProblemDetails)
				&& schema.Properties.TryGetValue("errors", out IOpenApiSchema? errors)
				&& errors is OpenApiSchema errorsSchema)
			{
				errorsSchema.Description = "Field name (camelCase) mapped to the error codes found for that field.";

				if (errorsSchema.AdditionalProperties is OpenApiSchema { Items: OpenApiSchema codes })
					codes.Enum = ToJsonValues(ValidationCodeValues);
			}

			return Task.CompletedTask;
		});

		options.AddOperationTransformer((operation, context, _) =>
		{
			IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;
			IAuthorizeData[] authorizeData = [.. metadata.OfType<IAuthorizeData>()];

			if (authorizeData.Length == 0 || metadata.OfType<IAllowAnonymous>().Any())
				return Task.CompletedTask;

			AddProblemResponse(operation, context, "401", "Missing, invalid or expired access token.");

			if (authorizeData.Any(data => data.Policy != AuthorizationPolicies.Authenticated))
				AddProblemResponse(operation, context, "403", "The terms have not been accepted.");

			return Task.CompletedTask;
		});

		return options;
	}

	private static void AddProblemResponse(
		OpenApiOperation operation,
		OpenApiOperationTransformerContext context,
		string statusCode,
		string description)
	{
		operation.Responses ??= new OpenApiResponses();
		operation.Responses[statusCode] = new OpenApiResponse
		{
			Description = description,
			Content = new Dictionary<string, OpenApiMediaType>
			{
				[ProblemJson] = new() { Schema = new OpenApiSchemaReference(ProblemDetailsSchemaId, context.Document) }
			}
		};
	}

	private static string[] ConstantValues(Type type) =>
	[
		.. type.GetFields(BindingFlags.Public | BindingFlags.Static)
			.Where(field => field.IsLiteral && field.FieldType == typeof(string))
			.Select(field => (string)field.GetRawConstantValue()!)
	];

	private static List<JsonNode> ToJsonValues(IEnumerable<string> values) =>
		[.. values.Select(value => (JsonNode)JsonValue.Create(value)!)];
}
