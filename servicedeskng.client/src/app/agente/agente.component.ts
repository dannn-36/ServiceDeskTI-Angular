import { Component, OnInit, OnDestroy, AfterViewChecked, ViewChild, ElementRef } from '@angular/core';
import { TicketsService, Ticket } from '../tickets/tickets.service';
import { ChatService } from '../chat/chat.service';

interface MensajeChat {
  remitente: string;
  texto: string;
  esAgente: boolean;
}

@Component({
  selector: 'app-agente',
  templateUrl: './agente.component.html',
  styleUrls: ['./agente.component.css']
})
export class AgenteComponent implements OnInit, OnDestroy, AfterViewChecked {
  tickets: Ticket[] = [];
  ticketSeleccionado: Ticket | null = null;
  mensajes: MensajeChat[] = [];
  mensajeTexto = '';
  usuarioNombre = '';
  usuarioId = 0;
  @ViewChild('chatScroll') chatScroll!: ElementRef;

  constructor(
    private ticketsService: TicketsService,
    private chatService: ChatService
  ) {}

  ngOnInit() {
    this.usuarioNombre = localStorage.getItem('usuario') || 'Agente';
    this.usuarioId = +(localStorage.getItem('usuarioId') || 0);
    this.cargarTicketsAsignados();
  }

  ngOnDestroy() {
    if (this.ticketSeleccionado) {
      this.chatService.disconnect(this.ticketSeleccionado.idTicket.toString());
    }
  }

  cargarTicketsAsignados() {
    const agenteId = +(localStorage.getItem('agenteId') || 0);
    this.ticketsService.getTicketsByAgente(agenteId).subscribe(tickets => {
      this.tickets = tickets;
    });
  }

  abrirChat(ticket: Ticket) {
    if (this.ticketSeleccionado) {
      this.chatService.disconnect(this.ticketSeleccionado.idTicket.toString());
    }
    this.ticketSeleccionado = ticket;
    this.mensajes = [];
    this.chatService.connect(ticket.idTicket.toString());
    this.chatService.onReceiveMessage((user, message, fecha) => {
      this.mensajes.push({
        remitente: user,
        texto: message,
        esAgente: user === this.usuarioNombre
      });
    });
  }

  enviarMensaje() {
    if (!this.mensajeTexto.trim() || !this.ticketSeleccionado) return;
    this.chatService.sendMessage(
      this.ticketSeleccionado.idTicket.toString(),
      this.usuarioNombre,
      this.mensajeTexto,
      this.usuarioId
    );
    this.mensajes.push({
      remitente: this.usuarioNombre,
      texto: this.mensajeTexto,
      esAgente: true
    });
    this.mensajeTexto = '';
  }

  cambiarEstadoTicket() {
    if (!this.ticketSeleccionado) return;
    this.ticketsService.update(this.ticketSeleccionado.idTicket, this.ticketSeleccionado).subscribe({
      next: () => {
        // Opcional: notificación visual
      },
      error: () => {
        // Opcional: manejo de error
      }
    });
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom() {
    try {
      this.chatScroll.nativeElement.scrollTop = this.chatScroll.nativeElement.scrollHeight;
    } catch (err) {}
  }
}
