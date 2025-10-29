import { Pipe, PipeTransform } from '@angular/core';
import { Ticket } from '../tickets/tickets.service';

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
    return tickets.filter(ticket => {
      const matchesBusqueda = busqueda === '' ||
        ticket.tituloTicket.toLowerCase().includes(busqueda.toLowerCase()) ||
        ticket.descripcionTicket?.toLowerCase().includes(busqueda.toLowerCase()) ||
        ticket.idTicket.toString().includes(busqueda);
      const matchesEstado = estado === '' || ticket.idEstadoTicket.toString() === estado;
      const matchesPrioridad = prioridad === '' || (ticket.prioridadTicket || '').toLowerCase() === prioridad.toLowerCase();
      const matchesCategoria = categoria === '' || ticket.idCategoriaTicket.toString() === categoria;
      return matchesBusqueda && matchesEstado && matchesPrioridad && matchesCategoria;
    });
  }
}
