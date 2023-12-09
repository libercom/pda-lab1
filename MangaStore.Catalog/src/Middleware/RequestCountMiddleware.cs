using Prometheus;

namespace MangaStore.Catalog.Middleware
{
    public class RequestCountMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly Counter RequestCounter = Metrics.CreateCounter("catalog_requests_total", "Count of requests to the Catalog API");

        public RequestCountMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Increment the counter for each incoming request
            RequestCounter.Inc();

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
