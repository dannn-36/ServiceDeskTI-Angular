import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Ticket {
  idTicket: number;
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
export class TicketsService {
  private apiUrl = 'http://localhost:5076/api/tickets';

  constructor(private http: HttpClient) { }

  // ✅ Obtener todos los tickets
  getAll(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(this.apiUrl);
  }

  // ✅ Obtener tickets por ID de usuario (usado en EndUserComponent)
  getTicketsByUser(userId: number): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.apiUrl}/usuario/${userId}`);
  }

  // ✅ Obtener un ticket por ID
  getById(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.apiUrl}/${id}`);
  }

  // ✅ Crear ticket (alias createTicket)
  createTicket(ticket: Partial<Ticket>): Observable<Ticket> {
    return this.http.post<Ticket>(this.apiUrl, ticket);
  }

  // ✅ Actualizar ticket
  update(id: number, ticket: Ticket): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, ticket);
  }

  // ✅ Eliminar ticket
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
