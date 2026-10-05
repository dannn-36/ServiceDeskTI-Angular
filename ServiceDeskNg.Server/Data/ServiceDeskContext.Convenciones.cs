using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ServiceDeskNg.Server.Data
{
    /// Convenciones añadidas al contexto generado por scaffolding.
    /// El archivo generado no se toca más allá de quitarle la cadena de conexión,
    /// para poder regenerarlo sin perder estas reglas.
    public partial class ServiceDeskContext
    {
        /// Todas las fechas viajan y se guardan en UTC.
        /// MySQL devuelve los TIMESTAMP sin zona (Kind = Unspecified); si no se marcan
        /// como UTC, System.Text.Json las serializa sin "Z" y el navegador las interpreta
        /// como hora local, desplazando todos los "hace X minutos" del panel.
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>().HaveConversion<FechaUtcConverter>();
            configurationBuilder.Properties<DateTime?>().HaveConversion<FechaUtcNullableConverter>();

            base.ConfigureConventions(configurationBuilder);
        }

        private sealed class FechaUtcConverter : ValueConverter<DateTime, DateTime>
        {
            public FechaUtcConverter()
                : base(
                    valor => valor.Kind == DateTimeKind.Local ? valor.ToUniversalTime() : valor,
                    valor => DateTime.SpecifyKind(valor, DateTimeKind.Utc))
            {
            }
        }

        private sealed class FechaUtcNullableConverter : ValueConverter<DateTime?, DateTime?>
        {
            public FechaUtcNullableConverter()
                : base(
                    valor => valor.HasValue && valor.Value.Kind == DateTimeKind.Local
                        ? valor.Value.ToUniversalTime()
                        : valor,
                    valor => valor.HasValue
                        ? DateTime.SpecifyKind(valor.Value, DateTimeKind.Utc)
                        : valor)
            {
            }
        }
    }
}
