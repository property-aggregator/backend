using System.Diagnostics;

namespace PropertyAggregator.Infrastructure.Geocoding;

internal sealed class RequestThrottle(TimeSpan interval)
{
	private readonly SemaphoreSlim _gate = new(1, 1);
	private long _nextAllowedTimestamp;

	public async Task WaitAsync(CancellationToken cancellationToken)
	{
		await _gate.WaitAsync(cancellationToken);

		try
		{
			TimeSpan delay = Stopwatch.GetElapsedTime(Stopwatch.GetTimestamp(), _nextAllowedTimestamp);

			if (delay > TimeSpan.Zero)
				await Task.Delay(delay, cancellationToken);

			_nextAllowedTimestamp = Stopwatch.GetTimestamp() + (long)(interval.TotalSeconds * Stopwatch.Frequency);
		}
		finally
		{
			_gate.Release();
		}
	}
}
