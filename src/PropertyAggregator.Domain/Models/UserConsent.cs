using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Domain.Models;

public sealed class UserConsent
{
	internal UserConsent(Guid userId, ConsentType type, string version, bool granted, DateTime createdAt)
	{
		Id = Guid.CreateVersion7();
		UserId = userId;
		Type = type;
		Version = version;
		Granted = granted;
		CreatedAt = createdAt;
	}

	private UserConsent()
	{
	}

	public Guid Id { get; private set; }
	public Guid UserId { get; private set; }
	public ConsentType Type { get; private set; }
	public string Version { get; private set; } = null!;
	public bool Granted { get; private set; }
	public DateTime CreatedAt { get; private set; }
}
