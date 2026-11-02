namespace PropertyAggregator.Application.Abstractions;

public sealed class UniqueConstraintViolationException(Exception innerException)
	: Exception("A unique constraint was violated.", innerException);
