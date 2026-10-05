namespace ServiceDeskNg.Server.Common
{
    /// Formatea antigüedades para los paneles ("45 min", "3 h", "2 d").
    /// Todas las comparaciones se hacen en UTC, que es como se guardan las fechas.
    public static class TiempoRelativo
    {
        public static string Formatear(DateTime? desdeUtc, DateTime? ahoraUtc = null)
        {
            if (desdeUtc is null)
                return "-";

            var transcurrido = (ahoraUtc ?? DateTime.UtcNow) - desdeUtc.Value;
            if (transcurrido < TimeSpan.Zero)
                transcurrido = TimeSpan.Zero;

            if (transcurrido.TotalMinutes < 60)
                return $"{(int)transcurrido.TotalMinutes} min";

            if (transcurrido.TotalHours < 24)
                return $"{(int)transcurrido.TotalHours} h";

            return $"{(int)transcurrido.TotalDays} d";
        }
    }
}
