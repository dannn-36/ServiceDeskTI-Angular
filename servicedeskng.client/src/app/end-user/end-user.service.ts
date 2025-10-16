import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EndUser {
  idEndUser: number;
  nombre: string;
  correo: string;
  rol: string;
  departamento: string;
  estado: string;
  ultimoAcceso: string;
}

@Injectable({ providedIn: 'root' })
export class EndUserService {
  private apiUrl = 'http://localhost:5076/api/enduser';

  constructor(private http: HttpClient) {}

  getAll(): Observable<EndUser[]> {
    return this.http.get<EndUser[]>(this.apiUrl);
  }

  getById(id: number): Observable<EndUser> {
    return this.http.get<EndUser>(`${this.apiUrl}/${id}`);
  }

  create(user: EndUser): Observable<EndUser> {
    return this.http.post<EndUser>(this.apiUrl, user);
  }

  update(id: number, user: EndUser): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, user);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
