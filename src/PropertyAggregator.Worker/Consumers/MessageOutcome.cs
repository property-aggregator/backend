namespace PropertyAggregator.Worker.Consumers;

public enum MessageOutcome
{
	Ack,
	Drop,
	Retry
}
