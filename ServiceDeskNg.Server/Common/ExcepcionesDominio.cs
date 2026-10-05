namespace ServiceDeskNg.Server.Common
{
    /// El usuario está autenticado pero no tiene permiso sobre este recurso concreto
    /// (por ejemplo, un cliente intentando leer el ticket de otro cliente).
    /// El manejador global la traduce a 403.
    public class AccesoDenegadoException : Exception
    {
        public AccesoDenegadoException(string mensaje = "No tiene permisos sobre este recurso.")
            : base(mensaje) { }
    }

    /// Regla de negocio violada con datos de entrada válidos; se traduce a 409.
    public class ConflictoNegocioException : Exception
    {
        public ConflictoNegocioException(string mensaje) : base(mensaje) { }
    }

    /// Una operación del servidor falló (por ejemplo, la herramienta de respaldo).
    /// Su mensaje lo redacta la aplicación y es seguro mostrarlo; el detalle técnico va al log.
    /// Se traduce a 500.
    public class FalloOperacionException : Exception
    {
        public FalloOperacionException(string mensajeParaElUsuario) : base(mensajeParaElUsuario) { }
    }
}
