import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { switchMap } from 'rxjs/operators';

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

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private apiUrl = '/api/usuario';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(this.apiUrl);
  }

  getById(id: number): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.apiUrl}/${id}`);
  }

  create(usuario: Usuario): Observable<Usuario> {
    return this.http.post<Usuario>(this.apiUrl, usuario);
  }

  update(id: number, usuario: Usuario): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, usuario);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Fix: fetch existing user and merge fields to avoid wiping departamento/estado when updating profile
  updateProfile(name: string, email: string): Observable<void> {
    const userId = +(localStorage.getItem('usuarioId') || 0);
    if (!userId) {
      throw new Error('No user id in localStorage');
    }

    return this.getById(userId).pipe(
      switchMap(existing => {
        const usuario: Usuario = {
          idUsuario: userId,
          nombreUsuario: name,
          correoUsuario: email,
          tipoUsuario: existing.tipoUsuario || localStorage.getItem('tipoUsuario') || '',
          departamentoUsuario: existing.departamentoUsuario,
          estadoUsuario: existing.estadoUsuario,
          ubicacionUsuario: existing.ubicacionUsuario,
          fechaHoraCreacionUsuario: existing.fechaHoraCreacionUsuario
        };
        return this.update(userId, usuario);
      })
    );
  }
}
