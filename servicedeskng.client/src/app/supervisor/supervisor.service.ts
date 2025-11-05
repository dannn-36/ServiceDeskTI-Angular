import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Supervisor {
  idSupervisor: number;
  idUsuario: number;
  idNivel: number;
}

export interface TeamMember {
  idAgente: number;
  name: string;
  status: string;
  tickets: number;
  avgTime: string;
  satisfaction: number;
}

export interface Ticket {
  id: string;
  title: string;
  user: string;
  agent: string;
  status: string;
  priority: string;
  category: string;
  time: string;
  fechaHoraCreacionTicket?: string;
  fechaHoraActualizacionTicket?: string;
  descripcion?: string;
}

export interface Escalation {
  id: string;
  title: string;
  escalatedTo: string;
  reason: string;
  time: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class SupervisorService {
  private apiUrl = '/api/supervisores';
  private teamUrl = '/api/team';
  private ticketsUrl = '/api/tickets';
  private priorityTicketsUrl = '/api/priority-tickets';
  private escalationsUrl = '/api/escalations';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Supervisor[]> {
    return this.http.get<Supervisor[]>(this.apiUrl);
  }

  getTeamMembers(): Observable<TeamMember[]> {
    return this.http.get<TeamMember[]>(this.teamUrl);
  }

  getTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(this.ticketsUrl);
  }

  getPriorityTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>('/api/priority-tickets');
  }

  getEscalations(): Observable<Escalation[]> {
    return this.http.get<Escalation[]>(this.escalationsUrl);
  }

  getById(id: number): Observable<Supervisor> {
    return this.http.get<Supervisor>(`${this.apiUrl}/${id}`);
  }

  create(supervisor: Supervisor): Observable<Supervisor> {
    return this.http.post<Supervisor>(this.apiUrl, supervisor);
  }

  update(id: number, supervisor: Supervisor): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, supervisor);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getDashboardTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>('/api/tickets/dashboard');
  }

  getWeeklyPerformance(): Observable<any> {
    return this.http.get<any>('/api/tickets/weekly-performance');
  }

  getAgentComparison(): Observable<any> {
    return this.http.get<any>('/api/team/comparison');
  }

  generateWeeklyReport(format: string) {
    return this.http.get(`/api/tickets/reporte-semanal?format=${format}`, { responseType: 'blob' });
  }

  generateIndividualReport(agente: string, format: string) {
    return this.http.get(`/api/tickets/reporte-individual?agente=${agente}&format=${format}`, { responseType: 'blob' });
  }

  generateSlaReport(format: string) {
    return this.http.get(`/api/tickets/reporte-sla?format=${format}`, { responseType: 'blob' });
  }

  getTicketsByAgente(agenteId: number): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`/api/tickets/agente/${agenteId}`);
  }
}
