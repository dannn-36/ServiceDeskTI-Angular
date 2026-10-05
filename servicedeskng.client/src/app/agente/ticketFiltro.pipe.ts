import { Pipe, PipeTransform } from '@angular/core';
import { Ticket } from '../tickets/tickets.service';
import { normalizarEstado } from '../core/modelos';

/**
 * Filtra la lista de tickets del agente.
 * Usa los nombres de estado y categoría que ya trae cada ticket desde la API,
 * en lugar de comparar ids escritos a mano en la plantilla.
 */
@Pipe({
  name: 'ticketFiltro'
})
export class TicketFiltroPipe implements PipeTransform {
  transform(
    tickets: Ticket[],
    busqueda: string,
    estado: string,
    prioridad: string,
    categoria: string
  ): Ticket[] {
    const texto = (busqueda ?? '').trim().toLowerCase();

    return tickets.filter(ticket => {
      const coincideBusqueda = !texto
        || ticket.tituloTicket.toLowerCase().includes(texto)
        || (ticket.descripcionTicket ?? '').toLowerCase().includes(texto)
        || ticket.idTicket.toString().includes(texto);

      const coincideEstado = !estado
        || normalizarEstado(ticket.nombreEstado) === normalizarEstado(estado);

      const coincidePrioridad = !prioridad
        || (ticket.prioridadTicket ?? '').toLowerCase() === prioridad.toLowerCase();

      const coincideCategoria = !categoria
        || (ticket.nombreCategoria ?? '').toLowerCase() === categoria.toLowerCase();

      return coincideBusqueda && coincideEstado && coincidePrioridad && coincideCategoria;
    });
  }
}
