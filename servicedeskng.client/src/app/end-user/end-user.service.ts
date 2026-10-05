import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ClienteResumen } from '../core/modelos';

@Injectable({ providedIn: 'root' })
export class EndUserService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/EndUser';

  /** Lista de clientes: la usa el personal para abrir tickets en su nombre. */
  getAll(): Observable<ClienteResumen[]> {
    return this.http.get<ClienteResumen[]>(this.apiUrl);
  }

  getById(id: number): Observable<ClienteResumen> {
    return this.http.get<ClienteResumen>(`${this.apiUrl}/${id}`);
  }
}
