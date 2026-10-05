import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface Agente {
  idAgente: number;
  idUsuario: number;
  idNivel: number;
  nombreUsuario: string;
  correoUsuario: string;
  especialidadAgente?: string;
  disponibilidadAgente: boolean;
}

@Injectable({ providedIn: 'root' })
export class AgenteService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/Agente';

  getById(id: number): Observable<Agente> {
    return this.http.get<Agente>(`${this.apiUrl}/${id}`);
  }

  /** El agente indica si puede recibir tickets nuevos en el reparto automático. */
  cambiarDisponibilidad(id: number, disponible: boolean): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/disponibilidad`, { disponible });
  }
}
