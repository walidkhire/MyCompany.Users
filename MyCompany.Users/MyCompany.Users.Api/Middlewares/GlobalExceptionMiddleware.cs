using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCompany.Users.Domain.Exceptions;
using MyCompany.Users.API.Helpers;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using static MyCompany.Users.Domain.Exceptions.AppException;

namespace MyCompany.Users.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // passe la requête au prochain middleware
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var traceId = context.TraceIdentifier;

            // 🔹 Définir le problème selon le type d'exception
            var problem = ex switch
            {
                ValidationException ve => ProblemDetailsFactory.CreateProblem(ve.StatusCode, "Erreur de validation", ve.Message, ve.ErrorCode, traceId, _env.IsDevelopment()),
                NotFoundException nf => ProblemDetailsFactory.CreateProblem(nf.StatusCode, "Ressource introuvable", nf.Message, nf.ErrorCode, traceId, _env.IsDevelopment()),
                UnauthorizedAccessException ua => ProblemDetailsFactory.CreateProblem(StatusCodes.Status401Unauthorized, "Non autorisé", ua.Message, "UNAUTHORIZED", traceId, _env.IsDevelopment()),
                ForbiddenException fe => ProblemDetailsFactory.CreateProblem(StatusCodes.Status403Forbidden, "Accès interdit", fe.Message, "FORBIDDEN", traceId, _env.IsDevelopment()),
                AppException ae => ProblemDetailsFactory.CreateProblem(ae.StatusCode, "Erreur métier", ae.Message, ae.ErrorCode, traceId, _env.IsDevelopment()),
                _ => ProblemDetailsFactory.CreateProblem(StatusCodes.Status500InternalServerError, "Erreur interne", "Une erreur inattendue est survenue", "INTERNAL_SERVER_ERROR", traceId, _env.IsDevelopment())
            };

            // 🔹 Logging complet
            _logger.LogError(ex, "Exception interceptée | {ErrorCode} | TraceId {TraceId}", problem.GetType().GetProperty("errorCode")?.GetValue(problem), traceId);

            // 🔹 Réponse HTTP
            context.Response.StatusCode = (int)problem.GetType().GetProperty("status")?.GetValue(problem)!;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}