import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Usuario {
<<<<<<< HEAD
  idUsuario?: number;
  nombreUsuario: string;
  correoUsuario: string;
  contrasenaUsuario?: string;
  departamentoUsuario?: string;
  estadoUsuario?: string;
  ubicacionUsuario?: string;
  fechaHoraCreacionUsuario?: string;
  rol?: string;
=======
  nombreUsuario: string;
  correoUsuario: string;
  contrasenaUsuario: string;
  tipoUsuario: string;
  departamentoUsuario?: string;
  estadoUsuario?: string;
  fechaHoraCreacionUsuario?: string; // <-- Agregado para mostrar la fecha
>>>>>>> 6b7e327f60382cd2a4d0540ced9dde7c873d8801
}

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private apiUrl = '/api/usuario';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(this.apiUrl);
  }

<<<<<<< HEAD
  create(usuario: Usuario, tipoUsuario: string): Observable<Usuario> {
    // Enviar tipoUsuario solo en la creación
    return this.http.post<Usuario>(this.apiUrl, { ...usuario, tipoUsuario });
  }

  update(id: number, usuario: Usuario): Observable<void> {
    const updateDto = {
      nombreUsuario: usuario.nombreUsuario,
      correoUsuario: usuario.correoUsuario,
      contrasenaUsuario: usuario.contrasenaUsuario,
      departamentoUsuario: usuario.departamentoUsuario,
      estadoUsuario: usuario.estadoUsuario,
      ubicacionUsuario: usuario.ubicacionUsuario
    };
    return this.http.put<void>(`${this.apiUrl}/${id}`, updateDto);
=======
  create(usuario: Usuario): Observable<Usuario> {
    return this.http.post<Usuario>(this.apiUrl, usuario);
  }

  update(id: number, usuario: Usuario): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, usuario);
>>>>>>> 6b7e327f60382cd2a4d0540ced9dde7c873d8801
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
