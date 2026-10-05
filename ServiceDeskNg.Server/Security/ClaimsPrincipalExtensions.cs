using System.Security.Claims;

namespace ServiceDeskNg.Server.Security
{
    /// Acceso tipado a la identidad del usuario autenticado.
    /// Toda autorización por dueño del recurso debe leer los datos desde aquí
    /// y nunca desde el body o la query de la petición.
    public static class ClaimsPrincipalExtensions
    {
        public static int IdUsuario(this ClaimsPrincipal user) =>
            LeerEntero(user, ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("La sesión no contiene un usuario válido.");

        public static int? IdSesion(this ClaimsPrincipal user) => LeerEntero(user, ClaimsApp.IdSesion);

        public static int? IdCliente(this ClaimsPrincipal user) => LeerEntero(user, ClaimsApp.IdCliente);

        public static int? IdAgente(this ClaimsPrincipal user) => LeerEntero(user, ClaimsApp.IdAgente);

        public static int? IdSupervisor(this ClaimsPrincipal user) => LeerEntero(user, ClaimsApp.IdSupervisor);

        public static string Nombre(this ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";

        public static string? Rol(this ClaimsPrincipal user) => user.FindFirst(ClaimTypes.Role)?.Value;

        public static bool EsAdministrador(this ClaimsPrincipal user) => user.IsInRole(RolesApp.Administrador);

        public static bool EsSupervisor(this ClaimsPrincipal user) => user.IsInRole(RolesApp.Supervisor);

        public static bool EsAgente(this ClaimsPrincipal user) => user.IsInRole(RolesApp.Agente);

        public static bool EsCliente(this ClaimsPrincipal user) => user.IsInRole(RolesApp.Cliente);

        /// Administración y supervisión ven la operación completa.
        public static bool TieneVisionGlobal(this ClaimsPrincipal user) =>
            user.EsAdministrador() || user.EsSupervisor();

        private static int? LeerEntero(ClaimsPrincipal user, string tipo) =>
            int.TryParse(user.FindFirst(tipo)?.Value, out var valor) ? valor : null;
    }
}
