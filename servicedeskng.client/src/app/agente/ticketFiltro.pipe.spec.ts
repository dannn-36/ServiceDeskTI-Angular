import { TicketFiltroPipe } from './ticketFiltro.pipe';
import { Ticket } from '../tickets/tickets.service';

const ticket = (parcial: Partial<Ticket>): Ticket => ({
  idTicket: 1,
  idCliente: 1,
  idEstadoTicket: 1,
  idCategoriaTicket: 1,
  tituloTicket: 'Sin título',
  descripcionTicket: '',
  ...parcial
});

describe('TicketFiltroPipe', () => {
  const pipe = new TicketFiltroPipe();

  const tickets: Ticket[] = [
    ticket({ idTicket: 10, tituloTicket: 'Impresora atascada', nombreEstado: 'Abierto', prioridadTicket: 'alta', nombreCategoria: 'Hardware' }),
    ticket({ idTicket: 11, tituloTicket: 'VPN no conecta', nombreEstado: 'En Progreso', prioridadTicket: 'urgente', nombreCategoria: 'Red' }),
    ticket({ idTicket: 12, tituloTicket: 'Licencia de Office', nombreEstado: 'Resuelto', prioridadTicket: 'baja', nombreCategoria: 'Software' })
  ];

  it('sin filtros devuelve todo', () => {
    expect(pipe.transform(tickets, '', '', '', '').length).toBe(3);
  });

  it('filtra por estado aunque la base lo escriba distinto ("En Progreso" / "en-progreso")', () => {
    const resultado = pipe.transform(tickets, '', 'en-progreso', '', '');
    expect(resultado.map(t => t.idTicket)).toEqual([11]);
  });

  it('combina búsqueda, prioridad y categoría', () => {
    expect(pipe.transform(tickets, 'impresora', '', 'alta', 'hardware').map(t => t.idTicket)).toEqual([10]);
    expect(pipe.transform(tickets, 'impresora', '', 'urgente', '').length).toBe(0);
  });

  it('busca también por número de ticket', () => {
    expect(pipe.transform(tickets, '12', '', '', '').map(t => t.idTicket)).toEqual([12]);
  });
});
