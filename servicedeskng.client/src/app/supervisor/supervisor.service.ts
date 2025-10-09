import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Supervisor {
  idSupervisor: number;
  idUsuario: number;
  idNivel: number;
}

@Injectable({ providedIn: 'root' })
export class SupervisorService {
  private apiUrl = '/api/supervisores';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Supervisor[]> {
    return this.http.get<Supervisor[]>(this.apiUrl);
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
}
