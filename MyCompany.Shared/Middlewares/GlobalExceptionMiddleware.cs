using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyCompany.Shared.Exceptions;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Hosting;


namespace MyCompany.Shared.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await Handle(context, ex);
            }
        }

        private async Task Handle(HttpContext context, Exception ex)
        {
            int statusCode = StatusCodes.Status500InternalServerError;
            string code = "INTERNAL_ERROR";
            string message = "Une erreur interne est survenue";

            if (ex is AppException app)
            {
                statusCode = app.StatusCode;
                code = app.ErrorCode;
                message = app.Message;
            }
            else if (ex is HttpRequestException)
            {
                statusCode = StatusCodes.Status503ServiceUnavailable;
                code = "SERVICE_UNAVAILABLE";
                message = "Service externe indisponible";
            }

            _logger.LogError(ex, "Erreur interceptée");

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = message,
                code,
                trace = _env.IsDevelopment() ? ex.StackTrace : null
            }));
        }
    }
}
