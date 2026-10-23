using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Abstractions;

public interface IUserRepository
{
	Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);

	Task<User> GetOrCreateAsync(string externalId, DateTime now, CancellationToken cancellationToken);
}
