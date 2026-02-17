using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyCompany.Shared.Middlewares
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var start = DateTime.UtcNow;
            await _next(context);
            var duration = DateTime.UtcNow - start;
            _logger.LogInformation("Requête {method} {url} exécutée en {duration} ms",
                context.Request.Method, context.Request.Path, duration.TotalMilliseconds);
        }
    }
}
