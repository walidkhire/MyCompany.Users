using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MyCompany.Gateway.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Erreur interceptée par la Gateway : {Message}", exception.Message);

            // Par défaut : Erreur 500
            int statusCode = StatusCodes.Status500InternalServerError;
            string errorCode = "INTERNAL_ERROR";
            string message = "Une erreur interne est survenue sur la Gateway.";

            // Si la Gateway n'arrive pas à joindre un microservice (ex: Users.API est éteint)
            if (exception is HttpRequestException || exception.InnerException is HttpRequestException)
            {
                statusCode = StatusCodes.Status503ServiceUnavailable;
                errorCode = "MICROSERVICE_UNAVAILABLE";
                message = "Un microservice interne est indisponible. Veuillez réessayer plus tard.";
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            var responseObj = new ProblemDetails
            {
                Status = statusCode,
                Title = errorCode,
                Detail = message,
                Instance = httpContext.Request.Path.Value
            };

            // On ajoute la StackTrace uniquement en mode Développement pour la sécurité
            if (_env.IsDevelopment())
            {
                responseObj.Extensions.Add("trace", exception.StackTrace);
            }

            await httpContext.Response.WriteAsJsonAsync(responseObj, cancellationToken);

            return true; // L'erreur est consommée et gérée
        }
    }
}