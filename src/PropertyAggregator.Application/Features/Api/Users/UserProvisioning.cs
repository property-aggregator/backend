using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Features.Api.Users;

internal static class UserProvisioning
{
	public static async Task<User> GetOrAddAsync(
		IUserRepository users,
		string externalId,
		DateTime now,
		CancellationToken cancellationToken)
	{
		User? user = await users.GetByExternalIdAsync(externalId, cancellationToken);

		if (user is not null)
			return user;

		user = new User(externalId, now);
		users.Add(user);

		return user;
	}
}
