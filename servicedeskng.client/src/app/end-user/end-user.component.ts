import { Component, OnInit, OnDestroy } from '@angular/core';
import { TicketsService, Ticket } from '../tickets/tickets.service';
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

  // Filtro de estado para tickets
  filtroEstado: string = '';
  ticketsFiltrados: Ticket[] = [];

  constructor(
    private ticketService: TicketsService,
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
    if (this.ticketSeleccionado) {
      this.chatService.disconnect(this.ticketSeleccionado.idTicket!.toString());
    }
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
    const clienteId = +(localStorage.getItem('clienteId') || 0);
    this.ticketService.getTicketsByUser(clienteId).subscribe(tickets => {
      this.tickets = tickets;
      this.filtrarTickets();
    });
  }

  filtrarTickets() {
    if (!this.filtroEstado) {
      this.ticketsFiltrados = this.tickets;
    } else {
      this.ticketsFiltrados = this.tickets.filter(t => this.getNombreEstado(t.idEstadoTicket).toLowerCase() === this.filtroEstado.toLowerCase());
    }
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
    const clienteId = +(localStorage.getItem('clienteId') || 0);
    const ticketData = {
      idCliente: clienteId,
      idEstadoTicket: estadoObj.idEstado,
      idCategoriaTicket: categoriaObj.idCategoria,
      tituloTicket: this.nuevoTicket.asunto,
      descripcionTicket: this.nuevoTicket.descripcion,
      prioridadTicket: 'media',
      ubicacionTicket: '',
      departamentoTicket: ''
    };
    // Solo enviar los campos primitivos, sin propiedades de navegación
    console.log('Datos enviados al backend:', ticketData);
    this.ticketService.createTicket(ticketData).subscribe({
      next: ticket => {
        this.tickets.unshift(ticket);
        this.cerrarModalTicket();
        this.abrirChat(ticket); // Abrir el chat automáticamente al crear el ticket
      },
      error: err => {
        console.error('Error al crear ticket:', err.error);
        alert('Error al crear ticket: ' + JSON.stringify(err.error?.errors || err.error));
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
    // Desconecta sesión anterior si hay
    if (this.ticketSeleccionado) {
      this.chatService.disconnect(this.ticketSeleccionado.idTicket.toString());
    }
    this.ticketSeleccionado = ticket;
    this.mensajes = [];

    // Conecta SIEMPRE al nuevo chat (no depende de ticketSeleccionado)
    this.chatService.connect(ticket.idTicket.toString());

    // Suscríbete a los mensajes recibidos SOLO una vez por conexión
    this.chatService.onReceiveMessage((user, message, fecha) => {
      console.log('Mensaje recibido:', user, message, fecha); // Depuración
      this.mensajes.push({
        remitente: user,
        texto: message,
        esCliente: user === this.usuarioNombre
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
      esCliente: true
    });

    this.mensajeTexto = '';
  }

  showProfileModal() {
    // TODO: aquí puedes abrir un modal o mostrar el perfil del usuario
    console.log('Abrir modal de perfil');
  }

  confirmLogout() {
    // TODO: lógica para cerrar sesión (borrar localStorage y redirigir)
    console.log('Cerrar sesión');
    localStorage.clear();
    window.location.href = '/'; // o usar el router: this.router.navigate(['/login']);
  }

}
