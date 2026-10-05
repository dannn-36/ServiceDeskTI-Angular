import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { CategoriaTicket, EstadoTicket } from './modelos';

/**
 * Catálogos de tickets compartidos por todos los paneles.
 * Se piden una sola vez y se reutilizan (antes cada componente los volvía a pedir
 * y además esperaba con un setTimeout de 200 ms a que "seguramente" hubieran llegado).
 */
@Injectable({ providedIn: 'root' })
export class CatalogoService {
  private readonly http = inject(HttpClient);

  readonly estados$: Observable<EstadoTicket[]> = this.http
    .get<EstadoTicket[]>('/api/tickets/estados')
    .pipe(shareReplay({ bufferSize: 1, refCount: false }));

  readonly categorias$: Observable<CategoriaTicket[]> = this.http
    .get<CategoriaTicket[]>('/api/tickets/categorias')
    .pipe(shareReplay({ bufferSize: 1, refCount: false }));
}
