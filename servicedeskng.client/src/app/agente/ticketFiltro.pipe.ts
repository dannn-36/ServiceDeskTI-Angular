import { Pipe, PipeTransform } from '@angular/core';
import { Ticket } from '../tickets/tickets.service';

@Pipe({
  name: 'ticketFiltro'
})
export class TicketFiltroPipe implements PipeTransform {
  categorias: any[] = [];

  // Método para obtener el nombre de la categoría
  getCategoriaNombre(idCategoria: number): string {
    const categoria = this.categorias.find(c => c.idCategoria === idCategoria);
    return categoria ? categoria.nombreCategoria : 'Otro';
  }

  transform(
    tickets: Ticket[],
    busqueda: string,
    estado: string,
    prioridad: string,
    categoria: string,
    categorias: any[]
  ): Ticket[] {
    this.categorias = categorias || [];
    return tickets.filter(ticket => {
      const matchesBusqueda = busqueda === '' ||
        ticket.tituloTicket.toLowerCase().includes(busqueda.toLowerCase()) ||
        ticket.descripcionTicket?.toLowerCase().includes(busqueda.toLowerCase()) ||
        ticket.idTicket.toString().includes(busqueda);
      const matchesEstado = estado === '' || ticket.idEstadoTicket.toString() === estado;
      const matchesPrioridad = prioridad === '' || (ticket.prioridadTicket || '').toLowerCase() === prioridad.toLowerCase();
      const matchesCategoria = categoria === '' || this.getCategoriaNombre(ticket.idCategoriaTicket).toLowerCase() === categoria.toLowerCase();
      return matchesBusqueda && matchesEstado && matchesPrioridad && matchesCategoria;
    });
  }
}
