using Prometheus;
using System.Diagnostics.Metrics;

namespace MangaStore.Catalog.Middleware
{
    public class RequestCountMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly Counter RequestCounter = Metrics.CreateCounter("inventory_requests_total", "Count of requests to the Inventory API");

        public RequestCountMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            RequestCounter.Inc();

            await _next(context);
        }
    }
}
