using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MyCompany.Users.API.HealthChecks
{
    public class GaiaVeApiHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GaiaVeApiHealthCheck> _logger;
        private const string TestUrl = "https://api.gaia-ve.com/status"; // mettre l'URL de ton API

        public GaiaVeApiHealthCheck(IHttpClientFactory httpClientFactory, ILogger<GaiaVeApiHealthCheck> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(TestUrl, cancellationToken);
                if (response.IsSuccessStatusCode)
                    return HealthCheckResult.Healthy("GaiaVe API reachable");

                return HealthCheckResult.Degraded($"GaiaVe API returned status {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GaiaVe API health check failed");
                return HealthCheckResult.Unhealthy("GaiaVe API unreachable", ex);
            }
        }
    }
}
