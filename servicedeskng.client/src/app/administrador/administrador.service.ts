import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface Administrador {
  idAdmin: number;
  idUsuario: number;
  idNivel: number;
  nombreUsuario: string;
  correoUsuario: string;
  areaResponsabilidadAdmin?: string;
}

@Injectable({ providedIn: 'root' })
export class AdministradorService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/Administrador';

  getAll(): Observable<Administrador[]> {
    return this.http.get<Administrador[]>(this.apiUrl);
  }

  getById(id: number): Observable<Administrador> {
    return this.http.get<Administrador>(`${this.apiUrl}/${id}`);
  }
}
