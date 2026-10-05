import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { Rol } from './modelos';

/**
 * Solo deja entrar a usuarios con alguno de los roles indicados.
 * Sin sesión se va al login; con otro rol, a su propio panel.
 *
 * Se usa como `canMatch` en rutas con carga diferida: si el rol no corresponde,
 * el navegador ni siquiera descarga el código de ese panel.
 * Es una comodidad de navegación: la protección real la hace la API,
 * que valida el rol en cada petición.
 */
export function rolGuard(...roles: Rol[]): CanMatchFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const usuario = auth.usuario();

    if (!usuario) {
      return router.createUrlTree(['/hogar']);
    }

    return roles.includes(usuario.rol) ? true : router.createUrlTree([auth.rutaInicio()]);
  };
}

/** La pantalla de login solo tiene sentido sin sesión: si ya hay una, lleva al panel. */
export const invitadoGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.usuario() ? inject(Router).createUrlTree([auth.rutaInicio()]) : true;
};
