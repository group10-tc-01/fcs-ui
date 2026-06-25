using Polly;
using System.Net;

namespace Fcs.UI.Infrastructure.Polly;

public static class PollyPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> DonationPolicy()
    {
        var retry = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => (int)msg.StatusCode >= 500 || msg.StatusCode == HttpStatusCode.ServiceUnavailable)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)));

        var circuit = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => (int)msg.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(30));

        return Policy.WrapAsync(retry, circuit);
    }

    public static IAsyncPolicy<HttpResponseMessage> DefaultRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => (int)msg.StatusCode >= 500)
            .WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(300));
    }
}
