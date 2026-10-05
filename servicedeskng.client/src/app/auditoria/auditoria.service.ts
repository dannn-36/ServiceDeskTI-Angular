import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RegistroAuditoria } from '../core/modelos';

/** Consulta de la bitácora de auditoría (solo lectura; la escribe el servidor). */
@Injectable({ providedIn: 'root' })
export class AuditoriaService {
  private readonly http = inject(HttpClient);

  listar(limite = 200, accion = ''): Observable<RegistroAuditoria[]> {
    let parametros = new HttpParams().set('limite', limite);
    if (accion) {
      parametros = parametros.set('accion', accion);
    }
    return this.http.get<RegistroAuditoria[]>('/api/auditoria', { params: parametros });
  }
}
