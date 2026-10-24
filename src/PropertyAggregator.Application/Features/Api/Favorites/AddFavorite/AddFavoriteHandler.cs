using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Features.Api.Favorites.AddFavorite;

public sealed class AddFavoriteHandler(
	IUserRepository users,
	IPropertyRepository properties,
	IUnitOfWork unitOfWork,
	TimeProvider timeProvider) : ICommandHandler<AddFavoriteCommand, AddFavoriteOutcome>
{
	public async Task<AddFavoriteOutcome> HandleAsync(AddFavoriteCommand command, CancellationToken cancellationToken)
	{
		if (!await properties.ExistsAsync(command.PropertyId, cancellationToken))
			return AddFavoriteOutcome.PropertyNotFound;

		DateTime now = timeProvider.GetUtcNow().UtcDateTime;

		User user = await users.GetWithFavoritesAsync(command.ExternalId, cancellationToken)
			?? throw new InvalidOperationException("The user was not found; the terms policy should have rejected the request.");

		AddFavoriteResult result = user.AddFavorite(command.PropertyId, now);

		if (result != AddFavoriteResult.Added)
			return result == AddFavoriteResult.AlreadyFavorite
				? AddFavoriteOutcome.AlreadyFavorite
				: AddFavoriteOutcome.LimitReached;

		return await TrySaveAsync(cancellationToken)
			? AddFavoriteOutcome.Added
			: AddFavoriteOutcome.AlreadyFavorite;
	}

	// A concurrent request (e.g. a double click) may have saved the same favorite first. The result is the same
	// as if it had already been there, so it is reported as such instead of failing.
	private async Task<bool> TrySaveAsync(CancellationToken cancellationToken)
	{
		try
		{
			await unitOfWork.SaveChangesAsync(cancellationToken);

			return true;
		}
		catch (UniqueConstraintViolationException)
		{
			return false;
		}
	}
}
