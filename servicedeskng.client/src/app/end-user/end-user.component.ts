import { Component, OnInit, OnDestroy } from '@angular/core';
import { TicketService, Ticket } from '../ticket/ticket.service';
import { ChatService } from '../chat/chat.service';
import { Subscription } from 'rxjs';
import { HttpClient } from '@angular/common/http';

interface MensajeChat {
  remitente: string;
  texto: string;
  esCliente: boolean;
}

@Component({
  selector: 'app-end-user',
  templateUrl: './end-user.component.html',
  styleUrls: ['./end-user.component.css']
})
export class EndUserComponent implements OnInit, OnDestroy {
  tickets: Ticket[] = [];
  nuevoTicket = { asunto: '', descripcion: '', categoria: '' };
  mostrarModalTicket = false;
  ticketSeleccionado: Ticket | null = null;
  mensajes: MensajeChat[] = [];
  mensajeTexto = '';
  usuarioNombre = '';
  usuarioId = 0;
  categoriaSeleccionada = '';
  private chatSub: Subscription | null = null;

  // Nuevos: para IDs de categorías y estados
  categorias: any[] = [];
  estados: any[] = [];

  constructor(
    private ticketService: TicketService,
    private chatService: ChatService,
    private http: HttpClient
  ) {}

  ngOnInit() {
    this.usuarioNombre = localStorage.getItem('usuario') || 'Cliente';
    this.usuarioId = +(localStorage.getItem('usuarioId') || 0);
    this.cargarCategorias();
    this.cargarEstados();
    this.cargarTickets();
  }

  ngOnDestroy() {
    if (this.chatSub) this.chatSub.unsubscribe();
    this.chatService.desconectar();
  }

  cargarCategorias() {
    this.http.get<any[]>('/api/tickets/categorias').subscribe(data => {
      this.categorias = data;
    });
  }

  cargarEstados() {
    this.http.get<any[]>('/api/tickets/estados').subscribe(data => {
      this.estados = data;
    });
  }

  abrirModalTicket(categoria: string = '') {
    this.mostrarModalTicket = true;
    this.categoriaSeleccionada = categoria;
    if (categoria) {
      this.nuevoTicket.categoria = categoria;
    }
  }

  cerrarModalTicket() {
    this.mostrarModalTicket = false;
    this.categoriaSeleccionada = '';
    this.nuevoTicket = { asunto: '', descripcion: '', categoria: '' };
  }

  cargarTickets() {
    this.ticketService.getTicketsByUser(this.usuarioId).subscribe(tickets => {
      this.tickets = tickets;
    });
  }

  crearTicket() {
    // Buscar el ID de la categoría seleccionada
    const categoriaObj = this.categorias.find(c => c.nombreCategoria === this.nuevoTicket.categoria);
    // Buscar el ID del estado "abierto"
    const estadoObj = this.estados.find(e => e.nombreEstado === 'abierto');
    // Log para depuración
    console.log('usuarioId:', this.usuarioId, 'asunto:', this.nuevoTicket.asunto, 'descripcion:', this.nuevoTicket.descripcion, 'categoria:', this.nuevoTicket.categoria, 'categoriaObj:', categoriaObj, 'estadoObj:', estadoObj);
    if (!categoriaObj || !estadoObj) {
      alert('No se pudo encontrar la categoría o el estado.');
      return;
    }
    // Validar campos requeridos
    if (!this.usuarioId || !this.nuevoTicket.asunto || !this.nuevoTicket.descripcion) {
      alert('Faltan campos requeridos.');
      return;
    }
    const ticketData = {
      idCliente: this.usuarioId,
      idEstadoTicket: estadoObj.idEstado,
      idCategoriaTicket: categoriaObj.idCategoria,
      tituloTicket: this.nuevoTicket.asunto,
      descripcionTicket: this.nuevoTicket.descripcion,
      prioridadTicket: 'media',
      ubicacionTicket: '',
      departamentoTicket: ''
    };
    this.ticketService.createTicket(ticketData).subscribe({
      next: ticket => {
        this.tickets.unshift(ticket);
        this.cerrarModalTicket();
      },
      error: err => {
        console.error('Error al crear ticket:', err);
        alert('Error al crear ticket: ' + (err.error?.message || err.message || 'Error desconocido'));
      }
    });
  }

  getNombreEstado(idEstado: number): string {
    const estado = this.estados.find(e => e.idEstado === idEstado);
    return estado ? estado.nombreEstado : idEstado;
  }

  getNombreCategoria(idCategoria: number): string {
    const categoria = this.categorias.find(c => c.idCategoria === idCategoria);
    return categoria ? categoria.nombreCategoria : idCategoria;
  }

  abrirChat(ticket: Ticket) {
    this.ticketSeleccionado = ticket;
    this.mensajes = [];
    this.chatService.desconectar();
    this.chatService.conectar(ticket.idTicket!);
    if (this.chatSub) this.chatSub.unsubscribe();
    this.chatSub = this.chatService.mensajes$.subscribe(m => {
      this.mensajes.push(m);
    });
  }

  enviarMensaje() {
    if (!this.mensajeTexto.trim() || !this.ticketSeleccionado) return;
    this.chatService.enviarMensaje(
      this.ticketSeleccionado.idTicket!,
      this.usuarioNombre,
      this.mensajeTexto,
      true
    );
    this.mensajes.push({ remitente: this.usuarioNombre, texto: this.mensajeTexto, esCliente: true });
    this.mensajeTexto = '';
  }

  showProfileModal() {
    // Implementa la lógica para mostrar el modal de perfil si lo necesitas
  }

  confirmLogout() {
    // Implementa la lógica para cerrar sesión si lo necesitas
  }
}
