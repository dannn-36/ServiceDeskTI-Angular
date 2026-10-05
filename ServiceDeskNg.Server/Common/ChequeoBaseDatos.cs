using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceDeskNg.Server.Data;

namespace ServiceDeskNg.Server.Common
{
    /// Comprobación de salud para /health: la API solo está sana si llega a la base de datos.
    /// La usan Docker, los balanceadores y la monitorización.
    public class ChequeoBaseDatos : IHealthCheck
    {
        private readonly ServiceDeskContext _context;

        public ChequeoBaseDatos(ServiceDeskContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Database.CanConnectAsync(cancellationToken)
                    ? HealthCheckResult.Healthy("Base de datos accesible.")
                    : HealthCheckResult.Unhealthy("Sin conexión con la base de datos.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Sin conexión con la base de datos.", ex);
            }
        }
    }
}
