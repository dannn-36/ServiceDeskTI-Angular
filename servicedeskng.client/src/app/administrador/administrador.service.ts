import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs'; 

export interface Administrador {
  idAdmin: number;
  idUsuario: number;
  idNivel: number;
  areaResponsabilidadAdmin?: string;
}

@Injectable({ providedIn: 'root' })
export class AdministradorService {
  private apiUrl = '/api/administradores';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Administrador[]> {
    return this.http.get<Administrador[]>(this.apiUrl);
  }

  getById(id: number): Observable<Administrador> {
    return this.http.get<Administrador>(`${this.apiUrl}/${id}`);
  }

  create(admin: Administrador): Observable<Administrador> {
    return this.http.post<Administrador>(this.apiUrl, admin);
  }

  update(id: number, admin: Administrador): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, admin);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
