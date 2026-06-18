using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCompany.Users.Domain.Exceptions;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static MyCompany.Users.Domain.Exceptions.AppException;

namespace MyCompany.Users.API.Infrastructure
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
            var traceId = httpContext.TraceIdentifier;

            // 1️⃣ Extraction propre des données de l'exception (Pas de réflexion !)
            int statusCode = StatusCodes.Status500InternalServerError;
            string title = "Erreur interne";
            string errorCode = "INTERNAL_SERVER_ERROR";
            string message = "Une erreur inattendue est survenue";

            switch (exception)
            {
                case ValidationException ve:
                    statusCode = ve.StatusCode;
                    title = "Erreur de validation";
                    errorCode = ve.ErrorCode;
                    message = ve.Message;
                    break;

                case NotFoundException nf:
                    statusCode = nf.StatusCode;
                    title = "Ressource introuvable";
                    errorCode = nf.ErrorCode;
                    message = nf.Message;
                    break;

                case UnauthorizedAccessException ua:
                    statusCode = StatusCodes.Status401Unauthorized;
                    title = "Non autorisé";
                    errorCode = "UNAUTHORIZED";
                    message = ua.Message;
                    break;

                case ForbiddenException fe:
                    statusCode = fe.StatusCode;
                    title = "Accès interdit";
                    errorCode = "FORBIDDEN";
                    message = fe.Message;
                    break;

                case AppException ae: // Capture toutes les autres exceptions héritées de AppException
                    statusCode = ae.StatusCode;
                    title = "Erreur métier";
                    errorCode = ae.ErrorCode;
                    message = ae.Message;
                    break;

                default:
                    // Reste sur l'erreur 500 par défaut
                    break;
            }

            // 2️⃣ Logging structuré à haute performance
            _logger.LogError(exception, "Exception interceptée | {ErrorCode} | TraceId: {TraceId}", errorCode, traceId);



            var detailMessage = exception.Message;
            if (exception.InnerException != null)
            {
                detailMessage += " ---> " + exception.InnerException.Message;
            }


            // 3️⃣ Construction de la réponse au standard IETF (RFC 7807) avec ProblemDetails
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detailMessage,
                Instance = httpContext.Request.Path.Value
            };

            // Ajout des extensions personnalisées (votre code d'erreur et le traceId)
            problemDetails.Extensions.Add("errorCode", errorCode);
            problemDetails.Extensions.Add("traceId", traceId);

            if (_env.IsDevelopment())
            {
                problemDetails.Extensions.Add("trace", exception.StackTrace);
            }

            // 4️⃣ Envoi de la réponse HTTP de manière optimisée
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // Indique à .NET 8 que le problème a été géré
        }
    }
}