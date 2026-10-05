import { HttpClient, HttpEvent } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

/**
 * Respaldo y restauración de la base de datos (solo administración).
 *
 * Este servicio faltaba en el repositorio: la regla `Backup*` del .gitignore
 * ignoraba la carpeta `backup/` y el proyecto no compilaba desde un clon limpio.
 * Ahora vive en `core/` con un nombre que esa regla no atrapa.
 */
@Injectable({ providedIn: 'root' })
export class RespaldoService {
  private readonly http = inject(HttpClient);

  descargarRespaldo(): Observable<Blob> {
    return this.http.get('/api/backup', { responseType: 'blob' });
  }

  restaurarRespaldo(archivo: File): Observable<HttpEvent<unknown>> {
    const formulario = new FormData();
    formulario.append('archivo', archivo, archivo.name);

    return this.http.post('/api/backup/restore', formulario, {
      observe: 'events',
      reportProgress: true
    });
  }
}
