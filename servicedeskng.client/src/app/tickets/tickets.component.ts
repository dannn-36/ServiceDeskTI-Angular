import { Component, OnInit } from '@angular/core';
import { TicketsService, Ticket } from './tickets.service';

@Component({
  selector: 'app-tickets',
  templateUrl: './tickets.component.html',
  styleUrls: ['./tickets.component.css']
})
export class TicketsComponent implements OnInit {
  tickets: Ticket[] = [];
  selectedTicket: Ticket | null = null;
  newTicket: Partial<Ticket> = {};
  editMode = false;
  loading = false;
  error = '';

  constructor(private ticketsService: TicketsService) {}

  ngOnInit() {
    this.getTickets();
  }

  getTickets() {
    this.loading = true;
    this.ticketsService.getAll().subscribe({
      next: (data) => { this.tickets = data; this.loading = false; },
      error: (err) => { this.error = 'Error al cargar tickets'; this.loading = false; }
    });
  }

  selectTicket(ticket: Ticket) {
    this.selectedTicket = { ...ticket };
    this.editMode = false;
  }

  startEdit(ticket: Ticket) {
    this.selectedTicket = { ...ticket };
    this.editMode = true;
  }

  saveEdit() {
    if (!this.selectedTicket) return;
    this.ticketsService.update(this.selectedTicket.idTicket, this.selectedTicket).subscribe({
      next: () => { this.getTickets(); this.editMode = false; this.selectedTicket = null; },
      error: () => { this.error = 'Error al actualizar ticket'; }
    });
  }

  deleteTicket(ticket: Ticket) {
    if (!confirm('¿Eliminar este ticket?')) return;
    this.ticketsService.delete(ticket.idTicket).subscribe({
      next: () => this.getTickets(),
      error: () => { this.error = 'Error al eliminar ticket'; }
    });
  }

  createTicket() {
    this.ticketsService.createTicket(this.newTicket as Ticket).subscribe({
      next: () => { this.getTickets(); this.newTicket = {}; },
      error: () => { this.error = 'Error al crear ticket'; }
    });
  }

  cancelEdit() {
    this.editMode = false;
    this.selectedTicket = null;
  }
}
