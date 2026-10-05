import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, switchMap, tap } from 'rxjs';
import { AuthService } from '../core/auth.service';

export interface Usuario {
  idUsuario?: number;
  nombreUsuario: string;
  correoUsuario: string;
  contrasenaUsuario?: string;
  tipoUsuario: string;
  departamentoUsuario?: string;
  estadoUsuario?: string;
  ubicacionUsuario?: string;
  fechaHoraCreacionUsuario?: string;
}

/** Respuesta de DELETE /api/Usuario/{id}: se elimina o, si tiene historial, se desactiva. */
export interface ResultadoBaja {
  message: string;
  eliminado: boolean;
}

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly apiUrl = '/api/Usuario';

  getAll(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(this.apiUrl);
  }

  getById(id: number): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.apiUrl}/${id}`);
  }

  create(usuario: Usuario): Observable<Usuario> {
    return this.http.post<Usuario>(this.apiUrl, usuario);
  }

  update(id: number, usuario: Partial<Usuario>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, usuario);
  }

  delete(id: number): Observable<ResultadoBaja> {
    return this.http.delete<ResultadoBaja>(`${this.apiUrl}/${id}`);
  }

  /**
   * Actualiza nombre y correo del usuario con sesión iniciada.
   * Lee primero el perfil para no borrar departamento y ubicación.
   * El id sale de la sesión, no de localStorage.
   */
  updateProfile(nombre: string, correo: string): Observable<void> {
    const usuario = this.auth.usuario();
    if (!usuario) {
      throw new Error('No hay sesión iniciada.');
    }

    return this.getById(usuario.idUsuario).pipe(
      switchMap(actual => this.update(usuario.idUsuario, {
        nombreUsuario: nombre,
        correoUsuario: correo,
        departamentoUsuario: actual.departamentoUsuario,
        ubicacionUsuario: actual.ubicacionUsuario
      })),
      tap(() => this.auth.actualizarDatosLocales(nombre, correo))
    );
  }
}
