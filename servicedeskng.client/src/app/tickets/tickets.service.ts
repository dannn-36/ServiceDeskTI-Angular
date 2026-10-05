import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

/** Ticket tal como lo devuelve /api/tickets (TicketDto en el backend). */
export interface Ticket {
  idTicket: number;
  idCliente: number;
  idAgenteAsignado?: number | null;
  idEstadoTicket: number;
  idCategoriaTicket: number;
  tituloTicket: string;
  descripcionTicket: string;
  prioridadTicket?: string;
  ubicacionTicket?: string;
  departamentoTicket?: string;
  fechaHoraCreacionTicket?: string;
  fechaHoraActualizacionTicket?: string;
  nombreEstado?: string;
  nombreCategoria?: string;
  nombreCliente?: string;
  nombreAgente?: string | null;
}

/** Datos para abrir un ticket. Un cliente no indica idCliente: lo pone el servidor. */
export interface NuevoTicket {
  idCliente?: number;
  tituloTicket: string;
  descripcionTicket: string;
  idCategoriaTicket: number;
  idEstadoTicket?: number;
  prioridadTicket?: string;
  ubicacionTicket?: string;
  departamentoTicket?: string;
}

/** Campos editables de un ticket (TicketUpdateDto en el backend). */
export interface CambioTicket {
  tituloTicket: string;
  descripcionTicket: string;
  idEstadoTicket: number;
  idCategoriaTicket: number;
  prioridadTicket?: string;
  ubicacionTicket?: string;
  departamentoTicket?: string;
}

@Injectable({ providedIn: 'root' })
export class TicketsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/tickets';

  getAll(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(this.apiUrl);
  }

  getTicketsByUser(idCliente: number): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.apiUrl}/cliente/${idCliente}`);
  }

  getTicketsByAgente(idAgente: number): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.apiUrl}/agente/${idAgente}`);
  }

  getById(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.apiUrl}/${id}`);
  }

  createTicket(ticket: NuevoTicket): Observable<Ticket> {
    return this.http.post<Ticket>(this.apiUrl, ticket);
  }

  update(id: number, cambio: CambioTicket): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, cambio);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /** Construye el cambio a partir del ticket actual, sobrescribiendo lo indicado. */
  static cambioDesde(ticket: Ticket, cambios: Partial<CambioTicket>): CambioTicket {
    return {
      tituloTicket: ticket.tituloTicket,
      descripcionTicket: ticket.descripcionTicket,
      idEstadoTicket: ticket.idEstadoTicket,
      idCategoriaTicket: ticket.idCategoriaTicket,
      prioridadTicket: ticket.prioridadTicket || 'media',
      ubicacionTicket: ticket.ubicacionTicket,
      departamentoTicket: ticket.departamentoTicket,
      ...cambios
    };
  }
}
