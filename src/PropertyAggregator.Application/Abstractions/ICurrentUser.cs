namespace PropertyAggregator.Application.Abstractions;

public interface ICurrentUser
{
	string ExternalId { get; }
}
