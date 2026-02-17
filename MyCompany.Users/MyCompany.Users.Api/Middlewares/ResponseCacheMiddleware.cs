using Microsoft.Extensions.Caching.Memory;

namespace MyCompany.Users.API.Middlewares
{
    public class ResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public ResponseCacheMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            var key = context.Request.Path.ToString();
            if (_cache.TryGetValue(key, out string cachedResponse))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(cachedResponse);
                return;
            }

            // Capturer la réponse
            var originalBody = context.Response.Body;
            using var newBody = new MemoryStream();
            context.Response.Body = newBody;

            await _next(context);

            newBody.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(newBody).ReadToEndAsync();
            newBody.Seek(0, SeekOrigin.Begin);

            // Mettre en cache
            _cache.Set(key, responseBody, TimeSpan.FromMinutes(5));

            await newBody.CopyToAsync(originalBody);
        }
    }

}
