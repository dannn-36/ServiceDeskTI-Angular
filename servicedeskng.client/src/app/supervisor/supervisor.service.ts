import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ComparativaAgente, RendimientoSemanal } from '../core/modelos';

export interface TeamMember {
  idAgente: number;
  name: string;
  status: string;
  /** Tickets activos asignados. */
  tickets: number;
  /** Horas promedio de resolución, medidas. */
  avgTime: string;
  /** null: el sistema no mide satisfacción todavía. */
  satisfaction: number | null;
}

/** Ticket tal como lo devuelven los paneles de supervisión. */
export interface Ticket {
  id: number;
  title: string;
  user: string;
  agent: string;
  idAgenteAsignado?: number | null;
  status: string;
  priority: string;
  category: string;
  time: string;
  fechaHoraCreacionTicket?: string;
  fechaHoraActualizacionTicket?: string;
  descripcion?: string;
}

export interface Escalation {
  id: number;
  title: string;
  escalatedTo: string;
  reason: string;
  time: string;
  status: 'critical' | 'pending' | 'resolved';
}

export interface ResultadoOperacion {
  message: string;
}

@Injectable({ providedIn: 'root' })
export class SupervisorService {
  private readonly http = inject(HttpClient);

  getTeamMembers(): Observable<TeamMember[]> {
    return this.http.get<TeamMember[]>('/api/team');
  }

  getAgentComparison(): Observable<ComparativaAgente[]> {
    return this.http.get<ComparativaAgente[]>('/api/team/comparison');
  }

  getDashboardTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>('/api/tickets/dashboard');
  }

  getPriorityTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>('/api/priority-tickets');
  }

  getEscalations(): Observable<Escalation[]> {
    return this.http.get<Escalation[]>('/api/escalations');
  }

  getVencidos(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>('/api/tickets/vencidos');
  }

  getWeeklyPerformance(): Observable<RendimientoSemanal> {
    return this.http.get<RendimientoSemanal>('/api/tickets/weekly-performance');
  }

  getTicketsByAgente(idAgente: number): Observable<unknown[]> {
    return this.http.get<unknown[]>(`/api/tickets/agente/${idAgente}`);
  }

  asignarTicket(idTicket: number, idAgente: number): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>('/api/tickets/assign', { idTicket, idAgente });
  }

  escalarTicket(idTicket: number, nuevaCategoria: string): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>(`/api/tickets/${idTicket}/escalar`, { nuevaCategoria });
  }

  redistribuir(): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>('/api/tickets/redistribuir', {});
  }

  asignarSinAgente(): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>('/api/tickets/asignar-sin-agente', {});
  }

  reporteCarga(): Observable<Blob> {
    return this.http.get('/api/tickets/reporte-carga', { responseType: 'blob' });
  }

  reporteSemanal(): Observable<Blob> {
    return this.http.get('/api/tickets/reporte-semanal', { responseType: 'blob' });
  }

  reporteIndividual(idAgente: number): Observable<Blob> {
    return this.http.get(`/api/tickets/reporte-individual?idAgente=${idAgente}`, { responseType: 'blob' });
  }
}
