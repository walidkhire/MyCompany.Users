using Microsoft.AspNetCore.Builder;
using MyCompany.Shared.Middlewares;

namespace MyCompany.Shared.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
