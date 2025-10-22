import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Ticket {
  idTicket?: number;
  idCliente: number;
  idAgenteAsignado?: number;
  idEstadoTicket: number;
  idCategoriaTicket: number;
  tituloTicket: string;
  descripcionTicket: string;
  prioridadTicket?: string;
  ubicacionTicket?: string;
  departamentoTicket?: string;
  fechaHoraCreacionTicket?: string;
  fechaHoraActualizacionTicket?: string;
}

@Injectable({ providedIn: 'root' })
export class TicketService {
  private apiUrl = '/api/tickets';

  constructor(private http: HttpClient) {}

  getTicketsByUser(userId: number): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.apiUrl}/cliente/${userId}`);
  }

  createTicket(ticket: Partial<Ticket>): Observable<Ticket> {
    return this.http.post<Ticket>(this.apiUrl, ticket);
  }
}
