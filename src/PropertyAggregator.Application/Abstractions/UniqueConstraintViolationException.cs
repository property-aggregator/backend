namespace PropertyAggregator.Application.Abstractions;

// Raised by the unit of work when a save violates a unique constraint, so handlers can react without knowing the database.
public sealed class UniqueConstraintViolationException(Exception innerException)
	: Exception("A unique constraint was violated.", innerException);
