using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Features.Api.Favorites.RemoveFavorite;

public sealed class RemoveFavoriteHandler(IUserRepository users, IUnitOfWork unitOfWork)
	: ICommandHandler<RemoveFavoriteCommand>
{
	public async Task HandleAsync(RemoveFavoriteCommand command, CancellationToken cancellationToken)
	{
		User? user = await users.GetWithFavoritesAsync(command.ExternalId, cancellationToken);

		if (user is null)
			return;

		user.RemoveFavorite(command.PropertyId);

		await unitOfWork.SaveChangesAsync(cancellationToken);
	}
}
