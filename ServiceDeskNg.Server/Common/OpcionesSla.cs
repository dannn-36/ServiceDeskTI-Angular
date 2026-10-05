namespace ServiceDeskNg.Server.Common
{
    /// Parámetros del acuerdo de nivel de servicio, configurables en appsettings.
    /// Antes el límite de 48 horas estaba escrito a mano dentro de un controlador.
    public class OpcionesSla
    {
        public const string Seccion = "Sla";

        /// Horas desde la apertura tras las cuales un ticket sin resolver se considera vencido.
        public int HorasVencimiento { get; set; } = 48;
    }
}
