import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, firstValueFrom, tap } from 'rxjs';
import { RUTA_POR_ROL, SesionUsuario } from './modelos';

/**
 * Estado de la sesión en el frontend.
 *
 * La credencial real es una cookie HttpOnly que gestiona el navegador: JavaScript
 * no puede leerla, así que un XSS no puede robarla. Aquí solo se guarda en memoria
 * quién es el usuario, para pintar la interfaz. Antes se guardaban rol e ids en
 * localStorage y bastaba con editarlos a mano para "cambiar de rol".
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly usuarioActual = signal<SesionUsuario | null>(null);

  /** Usuario autenticado, o null si no hay sesión. */
  readonly usuario = this.usuarioActual.asReadonly();
  readonly autenticado = computed(() => this.usuarioActual() !== null);

  login(correoUsuario: string, contrasenaUsuario: string): Observable<SesionUsuario> {
    return this.http
      .post<SesionUsuario>('/api/auth/login', { correoUsuario, contrasenaUsuario })
      .pipe(tap(sesion => this.usuarioActual.set(sesion)));
  }

  /**
   * Se ejecuta al arrancar la aplicación (APP_INITIALIZER): si la cookie sigue
   * siendo válida, recupera la identidad y el usuario no tiene que volver a entrar.
   */
  async restaurarSesion(): Promise<void> {
    try {
      const sesion = await firstValueFrom(this.http.get<SesionUsuario>('/api/auth/me'));
      this.usuarioActual.set(sesion);
    } catch {
      this.usuarioActual.set(null);
    }
  }

  /** Cierra la sesión en el servidor (la invalida en base de datos) y vuelve al login. */
  logout(): void {
    this.http.post('/api/auth/logout', {}).subscribe({
      complete: () => this.terminarSesionLocal(),
      error: () => this.terminarSesionLocal()
    });
  }

  /** Olvida la sesión sin llamar al servidor (por ejemplo, cuando ya respondió 401). */
  terminarSesionLocal(): void {
    this.usuarioActual.set(null);
    void this.router.navigate(['/hogar']);
  }

  /** Refleja en la interfaz un cambio de nombre o correo ya guardado en el servidor. */
  actualizarDatosLocales(nombreUsuario: string, correoUsuario: string): void {
    const actual = this.usuarioActual();
    if (actual) {
      this.usuarioActual.set({ ...actual, nombreUsuario, correoUsuario });
    }
  }

  /** Pantalla de inicio del usuario actual. */
  rutaInicio(): string {
    const usuario = this.usuarioActual();
    return usuario ? RUTA_POR_ROL[usuario.rol] ?? '/hogar' : '/hogar';
  }
}
