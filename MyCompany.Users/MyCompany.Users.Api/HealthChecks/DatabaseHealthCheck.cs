using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCompany.Users.Domain.Entities;
using System.Diagnostics;

namespace MyCompany.Users.API.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseHealthCheck> _logger;
        private const int TimeoutSeconds = 30;

        public DatabaseHealthCheck(IOptions<AppSettings> options, ILogger<DatabaseHealthCheck> logger)
        {
            _connectionString = options.Value.ConnectionString;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds));

            try
            {
                var stopwatch = Stopwatch.StartNew();

                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(cts.Token);

                await using var cmd = new SqlCommand("SELECT 1", conn)
                {
                    CommandTimeout = TimeoutSeconds
                };
                await cmd.ExecuteScalarAsync(cts.Token);

                stopwatch.Stop();

                if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
                    return HealthCheckResult.Degraded($"Database reachable but slow ({stopwatch.ElapsedMilliseconds} ms)");

                return HealthCheckResult.Healthy($"Database reachable ({stopwatch.ElapsedMilliseconds} ms)");
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Database health check timed out");
                return HealthCheckResult.Unhealthy("Database timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return HealthCheckResult.Unhealthy("Database unreachable", ex);
            }
        }
    }
}
