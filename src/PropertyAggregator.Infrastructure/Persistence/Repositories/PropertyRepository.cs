using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Infrastructure.Persistence.Repositories;

internal sealed class PropertyRepository(AppDbContext db) : IPropertyRepository
{
	public Task<Property?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
		db.Properties.FirstOrDefaultAsync(property => property.Id == id, cancellationToken);

	public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
		db.Properties.AnyAsync(property => property.Id == id, cancellationToken);

	public Task<Property?> GetBySourceAsync(string sourcePortal, string sourceId, CancellationToken cancellationToken) =>
		db.Properties.FirstOrDefaultAsync(
			property => property.SourcePortal == sourcePortal && property.SourceId == sourceId,
			cancellationToken);

	public async Task<IReadOnlyList<Property>> ListPendingGeocodingAsync(int batchSize, CancellationToken cancellationToken) =>
		await db.Properties
			.Where(property => property.Location == null && property.GeocodedAt == null && property.City != null)
			.OrderByDescending(property => property.FirstSeenAt)
			.Take(batchSize)
			.ToListAsync(cancellationToken);

	public async Task<IReadOnlyList<Property>> ListStaleAsync(
		DateTime staleBefore,
		DateTime retryBefore,
		int batchSize,
		CancellationToken cancellationToken) =>
		await db.Properties
			.Where(property => property.ArchivedAt == null
				&& property.LastSeenAt < staleBefore
				&& (property.ExpirationCheckRequestedAt == null || property.ExpirationCheckRequestedAt < retryBefore))
			.OrderBy(property => property.LastSeenAt)
			.Take(batchSize)
			.ToListAsync(cancellationToken);

	public void Add(Property property) => db.Properties.Add(property);
}
