using Microsoft.AspNetCore.Http;

namespace MyCompany.Shared.Middlewares
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
            {
                await _next(context);
                return;
            }

            // Ici tu pourrais ajouter validation globale
            await _next(context);
        }
    }

}
