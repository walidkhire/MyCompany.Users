using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyCompany.Shared.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            
                // Log de la requête
                _logger.LogInformation("Requête {method} {url}", context.Request.Method, context.Request.Path);

                await _next(context); // passe au middleware suivant

                // Log de la réponse
                _logger.LogInformation("Réponse {statusCode}", context.Response.StatusCode);
             
        }
    }

}
