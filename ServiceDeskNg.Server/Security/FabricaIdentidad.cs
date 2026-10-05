using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using ServiceDeskNg.Server.Models.Dtos;

namespace ServiceDeskNg.Server.Security
{
    /// Convierte la identidad de un usuario en el principal que viaja dentro
    /// de la cookie cifrada, y en el DTO que recibe el frontend.
    public static class FabricaIdentidad
    {
        public static ClaimsPrincipal CrearPrincipal(IdentidadUsuario identidad, int idSesion)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, identidad.Usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, identidad.Usuario.NombreUsuario),
                new(ClaimTypes.Email, identidad.Usuario.CorreoUsuario),
                new(ClaimTypes.Role, identidad.Rol),
                new(ClaimsApp.IdSesion, idSesion.ToString())
            };

            AgregarSiTieneValor(claims, ClaimsApp.IdCliente, identidad.IdCliente);
            AgregarSiTieneValor(claims, ClaimsApp.IdAgente, identidad.IdAgente);
            AgregarSiTieneValor(claims, ClaimsApp.IdSupervisor, identidad.IdSupervisor);
            AgregarSiTieneValor(claims, ClaimsApp.IdAdministrador, identidad.IdAdministrador);

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role);

            return new ClaimsPrincipal(claimsIdentity);
        }

        public static SesionUsuarioDto CrearDto(IdentidadUsuario identidad) => new()
        {
            IdUsuario = identidad.Usuario.IdUsuario,
            NombreUsuario = identidad.Usuario.NombreUsuario,
            CorreoUsuario = identidad.Usuario.CorreoUsuario,
            Rol = identidad.Rol,
            IdCliente = identidad.IdCliente,
            IdAgente = identidad.IdAgente,
            IdSupervisor = identidad.IdSupervisor,
            IdAdministrador = identidad.IdAdministrador
        };

        private static void AgregarSiTieneValor(List<Claim> claims, string tipo, int? valor)
        {
            if (valor.HasValue)
                claims.Add(new Claim(tipo, valor.Value.ToString()));
        }
    }
}
