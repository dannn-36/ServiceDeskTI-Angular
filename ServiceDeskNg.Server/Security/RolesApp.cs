namespace ServiceDeskNg.Server.Security
{
    /// Nombres canónicos de los roles del sistema.
    /// Son los valores que se guardan en el claim de rol y los que espera el frontend.
    public static class RolesApp
    {
        public const string Administrador = "Administrador";
        public const string Supervisor = "Supervisor";
        public const string Agente = "Agente";
        public const string Cliente = "Cliente";

        // Combinaciones para [Authorize(Roles = ...)], que acepta una lista separada por comas.

        /// Quienes gestionan la operación completa del service desk.
        public const string Gestion = Administrador + "," + Supervisor;

        /// Personal interno (todo el que no es cliente).
        public const string Personal = Administrador + "," + Supervisor + "," + Agente;

        /// Valor de la columna niveles_acceso.nivel que corresponde a cada rol,
        /// tal como lo carga Database/DatabaseScript.txt (1 = básico ... 4 = total).
        /// Antes se buscaba por un nombre ("EndUser", "Agente"...) que no existe en el script,
        /// y como fallaba siempre, todos los usuarios recibían el nivel 1, incluidos los administradores.
        public static int NivelDeRol(string rol) => rol switch
        {
            Cliente => 1,
            Agente => 2,
            Supervisor => 3,
            Administrador => 4,
            _ => throw new ArgumentException($"Rol desconocido: {rol}")
        };

        public static bool EsRolValido(string? rol) =>
            rol is Administrador or Supervisor or Agente or Cliente;
    }

    /// Claims propios de la aplicación, además de los estándar de .NET.
    public static class ClaimsApp
    {
        public const string IdSesion = "servicedesk:idSesion";
        public const string IdCliente = "servicedesk:idCliente";
        public const string IdAgente = "servicedesk:idAgente";
        public const string IdSupervisor = "servicedesk:idSupervisor";
        public const string IdAdministrador = "servicedesk:idAdministrador";
    }
}
