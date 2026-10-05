using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using ServiceDeskNg.Server.Common;

namespace ServiceDeskNg.Tests.Unitarias
{
    public class ManejadorGlobalErroresTests
    {
        private static async Task<(int Estado, string Mensaje)> ManejarAsync(Exception excepcion)
        {
            var contexto = new DefaultHttpContext();
            contexto.Response.Body = new MemoryStream();

            var manejador = new ManejadorGlobalErrores(NullLogger<ManejadorGlobalErrores>.Instance);
            await manejador.TryHandleAsync(contexto, excepcion, CancellationToken.None);

            contexto.Response.Body.Position = 0;
            var cuerpo = await JsonDocument.ParseAsync(contexto.Response.Body);
            return (contexto.Response.StatusCode, cuerpo.RootElement.GetProperty("message").GetString()!);
        }

        [Theory]
        [InlineData(typeof(KeyNotFoundException), 404)]
        [InlineData(typeof(AccesoDenegadoException), 403)]
        [InlineData(typeof(UnauthorizedAccessException), 401)]
        [InlineData(typeof(ConflictoNegocioException), 409)]
        [InlineData(typeof(ArgumentException), 400)]
        public async Task ExcepcionesDelDominio_SeTraducenASuCodigoConSuMensaje(Type tipo, int estadoEsperado)
        {
            var excepcion = (Exception)Activator.CreateInstance(tipo, "Mensaje para el usuario")!;

            var (estado, mensaje) = await ManejarAsync(excepcion);

            Assert.Equal(estadoEsperado, estado);
            Assert.Equal("Mensaje para el usuario", mensaje);
        }

        [Fact]
        public async Task ErroresDeInfraestructura_NoFiltranSuMensajeInterno()
        {
            // EF Core lanza InvalidOperationException, por ejemplo, cuando MySQL no responde.
            var (estado, mensaje) = await ManejarAsync(
                new InvalidOperationException("Server=db-interna;User=root: transient failure"));

            Assert.Equal(500, estado);
            Assert.DoesNotContain("db-interna", mensaje);
            Assert.DoesNotContain("root", mensaje);
        }
    }
}
