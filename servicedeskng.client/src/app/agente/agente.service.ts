import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Agente {
  idAgente: number;
  idUsuario: number;
  idNivel: number;
  especialidadAgente?: string;
  disponibilidadAgente?: boolean;
}

@Injectable({ providedIn: 'root' })
export class AgenteService {
  private apiUrl = '/api/agentes';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Agente[]> {
    return this.http.get<Agente[]>(this.apiUrl);
  }

  getById(id: number): Observable<Agente> {
    return this.http.get<Agente>(`${this.apiUrl}/${id}`);
  }

  create(agente: Agente): Observable<Agente> {
    return this.http.post<Agente>(this.apiUrl, agente);
  }

  update(id: number, agente: Agente): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, agente);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
