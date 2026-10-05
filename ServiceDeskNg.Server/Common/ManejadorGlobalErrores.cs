using Microsoft.AspNetCore.Diagnostics;

namespace ServiceDeskNg.Server.Common
{
    /// Traduce las excepciones del dominio a códigos HTTP en un único lugar,
    /// para que los controladores no repitan try/catch.
    /// Nunca expone ex.Message al cliente en errores no previstos: eso se registra en el log.
    public class ManejadorGlobalErrores : IExceptionHandler
    {
        private readonly ILogger<ManejadorGlobalErrores> _logger;

        public ManejadorGlobalErrores(ILogger<ManejadorGlobalErrores> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (estado, mensaje) = Traducir(exception);

            if (estado >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Error no controlado en {Metodo} {Ruta}",
                    context.Request.Method,
                    context.Request.Path);
            }
            else
            {
                _logger.LogInformation(
                    "Petición rechazada ({Estado}) en {Metodo} {Ruta}: {Mensaje}",
                    estado,
                    context.Request.Method,
                    context.Request.Path,
                    exception.Message);
            }

            context.Response.StatusCode = estado;
            await context.Response.WriteAsJsonAsync(new { message = mensaje }, cancellationToken);
            return true;
        }

        /// Solo se muestra el mensaje de excepciones que lanza la propia aplicación.
        /// InvalidOperationException NO está en la lista a propósito: EF Core y el framework
        /// la usan para fallos de infraestructura (por ejemplo, base de datos caída) y su
        /// mensaje revela detalles internos.
        private static (int Estado, string Mensaje) Traducir(Exception exception) => exception switch
        {
            KeyNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            AccesoDenegadoException ex => (StatusCodes.Status403Forbidden, ex.Message),
            UnauthorizedAccessException ex => (StatusCodes.Status401Unauthorized, ex.Message),
            ConflictoNegocioException ex => (StatusCodes.Status409Conflict, ex.Message),
            FalloOperacionException ex => (StatusCodes.Status500InternalServerError, ex.Message),
            ArgumentException ex => (StatusCodes.Status400BadRequest, ex.Message),
            _ => (StatusCodes.Status500InternalServerError,
                  "Ocurrió un error inesperado. Revise los registros del servidor.")
        };
    }
}
