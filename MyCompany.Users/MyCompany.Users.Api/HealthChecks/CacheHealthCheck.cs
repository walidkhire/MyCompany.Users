using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MyCompany.Users.API.HealthChecks
{
    public class CacheHealthCheck : IHealthCheck
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheHealthCheck> _logger;

        public CacheHealthCheck(IMemoryCache memoryCache, ILogger<CacheHealthCheck> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                const string key = "healthcheck";
                _memoryCache.Set(key, "ok", TimeSpan.FromSeconds(5));
                if (_memoryCache.TryGetValue(key, out string? value) && value == "ok")
                {
                    return Task.FromResult(HealthCheckResult.Healthy("Cache is working."));
                }

                return Task.FromResult(HealthCheckResult.Degraded("Cache is not responding quickly."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache health check failed");
                return Task.FromResult(HealthCheckResult.Unhealthy("Cache is not working."));
            }
        }
    }
}
