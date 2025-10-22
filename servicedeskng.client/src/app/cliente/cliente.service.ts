import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
export interface cliente {
  idCliente: number;
  idUsuario?: number;
  IdNivel?: number;
 
}
@Injectable({
  providedIn: 'root'
})

export class ClienteService {

  private apiUrl = 'http://localhost:5076/api/clientes'; // URL de tu backend

  constructor(private http: HttpClient) { }

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  create(cliente: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, cliente);
  }

  update(id: number, cliente: any): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, cliente);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
