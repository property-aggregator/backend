namespace PropertyAggregator.Infrastructure.Geocoding;

internal sealed class ThrottlingHandler(RequestThrottle throttle) : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		await throttle.WaitAsync(cancellationToken);

		return await base.SendAsync(request, cancellationToken);
	}
}
