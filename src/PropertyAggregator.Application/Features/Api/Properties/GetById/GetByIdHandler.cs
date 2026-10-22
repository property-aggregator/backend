using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Features.Api.Properties.GetById;

public sealed class GetByIdHandler(IPropertyQueries queries) : IQueryHandler<GetByIdQuery, PropertyResult?>
{
	public Task<PropertyResult?> HandleAsync(GetByIdQuery query, CancellationToken cancellationToken) =>
		queries.GetByIdAsync(query.Id, cancellationToken);
}
