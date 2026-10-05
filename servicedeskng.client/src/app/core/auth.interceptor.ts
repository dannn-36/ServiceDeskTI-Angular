import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/** Peticiones cuyo 401 es una respuesta esperada y no significa "la sesión caducó". */
const RUTAS_SIN_REDIRECCION = ['/api/auth/login', '/api/auth/me'];

/**
 * Si el servidor responde 401 (sesión caducada, cerrada en otro sitio o usuario
 * desactivado), se limpia el estado local y se vuelve al login.
 */
export const authInterceptor: HttpInterceptorFn = (peticion, siguiente) => {
  const auth = inject(AuthService);

  return siguiente(peticion).pipe(
    catchError((error: unknown) => {
      const esperado = RUTAS_SIN_REDIRECCION.some(ruta => peticion.url.includes(ruta));

      if (error instanceof HttpErrorResponse && error.status === 401 && !esperado) {
        auth.terminarSesionLocal();
      }

      return throwError(() => error);
    })
  );
};
