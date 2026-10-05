using ServiceDeskNg.Server.Models;

namespace ServiceDeskNg.Server.Security
{
    /// Identidad completa de un usuario: su rol y el identificador que tiene
    /// en la tabla de ese rol.
    public sealed record IdentidadUsuario(
        Usuario Usuario,
        string Rol,
        int? IdCliente,
        int? IdAgente,
        int? IdSupervisor,
        int? IdAdministrador);

    /// Único lugar donde se decide el rol de un usuario.
    /// Antes esta misma cadena de if/else estaba repetida en tres sitios,
    /// y además con nombres distintos ("Cliente" en uno, "EndUser" en otro).
    public static class ResolucionRol
    {
        public const string SinRol = "Sin Rol";

        /// Requiere que las colecciones de roles vengan cargadas (Include).
        public static IdentidadUsuario Resolver(Usuario usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var admin = usuario.Administradores.FirstOrDefault();
            if (admin is not null)
                return new IdentidadUsuario(usuario, RolesApp.Administrador, null, null, null, admin.IdAdmin);

            var supervisor = usuario.Supervisores.FirstOrDefault();
            if (supervisor is not null)
                return new IdentidadUsuario(usuario, RolesApp.Supervisor, null, null, supervisor.IdSupervisor, null);

            var agente = usuario.Agentes.FirstOrDefault();
            if (agente is not null)
                return new IdentidadUsuario(usuario, RolesApp.Agente, null, agente.IdAgente, null, null);

            var cliente = usuario.Clientes.FirstOrDefault();
            if (cliente is not null)
                return new IdentidadUsuario(usuario, RolesApp.Cliente, cliente.IdCliente, null, null, null);

            return new IdentidadUsuario(usuario, SinRol, null, null, null, null);
        }

        public static string RolDe(Usuario usuario) => Resolver(usuario).Rol;
    }
}
