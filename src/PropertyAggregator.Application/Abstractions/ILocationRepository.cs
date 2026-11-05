namespace PropertyAggregator.Application.Abstractions;

public interface ILocationRepository
{
	Task<bool> IsImportedAsync(string version, CancellationToken cancellationToken);

	Task ImportAsync(string version, IReadOnlyList<LocationImportRow> rows, DateTime importedAt,
		CancellationToken cancellationToken);

	Task<int> MatchPropertiesAsync(DateTime matchedAt, CancellationToken cancellationToken);

	Task RefreshDistrictsAsync(CancellationToken cancellationToken);

	Task RefreshOfferCountsAsync(CancellationToken cancellationToken);
}
