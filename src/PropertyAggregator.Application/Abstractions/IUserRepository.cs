using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Abstractions;

public interface IUserRepository
{
	Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);

	void Add(User user);
}
