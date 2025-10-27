import { Component, OnInit, OnDestroy } from '@angular/core';
import { TicketsService, Ticket } from '../tickets/tickets.service';
import { ChatService } from '../chat/chat.service';
import * as signalR from '@microsoft/signalr';

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
export class AgenteComponent implements OnInit, OnDestroy {
  agente = {
    idAgente: 0,
    idUsuario: 0,
    idNivel: 0,
    especialidadAgente: '',
    disponibilidadAgente: false
  };

  tickets: Ticket[] = [];
  ticketSeleccionado: Ticket | null = null;
  mensajes: MensajeChat[] = [];
  mensajeTexto = '';
  usuarioNombre = '';
  usuarioId = 0;

  constructor(
    private ticketsService: TicketsService,
    private chatService: ChatService
  ) {}

  ngOnInit() {
    // Obtener datos del agente desde localStorage
    this.usuarioNombre = localStorage.getItem('usuario') || 'Agente';
    this.usuarioId = +(localStorage.getItem('usuarioId') || 0);
    this.agente.idAgente = +(localStorage.getItem('agenteId') || 0);
    this.agente.idUsuario = this.usuarioId;
    console.log('Agente ID:', this.agente.idAgente); // Depuración
    this.cargarTicketsAsignados();
  }

  ngOnDestroy() {
    if (this.ticketSeleccionado) {
      this.chatService.disconnect(this.ticketSeleccionado.idTicket.toString());
    }
  }

  cargarTicketsAsignados() {
    this.ticketsService.getTicketsByAgente(this.agente.idAgente).subscribe(tickets => {
      console.log('Tickets recibidos:', tickets); // Depuración
      this.tickets = tickets;
    });
  }

  abrirChat(ticket: Ticket) {
    // Desconecta sesión anterior si hay
    if (this.ticketSeleccionado) {
      this.chatService.disconnect(this.ticketSeleccionado.idTicket.toString());
    }
    this.ticketSeleccionado = ticket;
    this.mensajes = [];
    // Conecta al nuevo chat
    this.chatService.connect(ticket.idTicket.toString());
    // Suscríbete a los mensajes recibidos
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
    // Verifica que la conexión esté activa antes de enviar
    if (!this.chatService['hubConnection'] || this.chatService['hubConnection'].state !== signalR.HubConnectionState.Connected) {
      console.error('No hay conexión activa al chat.');
      return;
    }
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
}
