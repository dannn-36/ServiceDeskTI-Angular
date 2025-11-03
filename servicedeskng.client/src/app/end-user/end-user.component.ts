import { Component, OnInit, OnDestroy } from '@angular/core';
import { TicketsService, Ticket } from '../tickets/tickets.service';
import { ChatService } from '../chat/chat.service';
import { Subscription } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { UsuarioService } from '../usuario/usuario.service';

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

  categorias: any[] = [];
  estados: any[] = [];

  // Filtro de estado para tickets
  filtroEstado: string = '';
  ticketsFiltrados: Ticket[] = [];

  mostrarChatModal = false;

  mostrarProfileModal = false;
  profileName = '';
  profileEmail = '';

  constructor(
    private ticketService: TicketsService,
    private chatService: ChatService,
    private http: HttpClient,
    private usuarioService: UsuarioService // Inyectar el servicio de Usuario
  ) {}

  ngOnInit() {
    this.usuarioNombre = localStorage.getItem('usuario') || 'Cliente';
    this.usuarioId = +(localStorage.getItem('usuarioId') || 0);
    this.profileName = this.usuarioNombre;
    this.profileEmail = localStorage.getItem('usuarioEmail') || 'usuario@empresa.com';
    this.cargarCategorias();
    this.cargarEstados();
    this.cargarTickets();
  }

  ngOnDestroy() {
    if (this.mostrarChatModal) {
      this.cerrarChatModal();
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
    if (!this.nuevoTicket.categoria) {
      alert('Debes seleccionar una categoría.');
      return;
    }
    console.log('Categorias cargadas:', this.categorias);
    console.log('Valor seleccionado:', this.nuevoTicket.categoria);
    // Buscar el ID de la categoría seleccionada (normalizado)
    const categoriaObj = this.categorias.find(
      c => c.nombreCategoria.trim().toLowerCase() === this.nuevoTicket.categoria.trim().toLowerCase()
    );
    // Buscar el ID del estado "abierto"
    const estadoObj = this.estados.find(e => e.nombreEstado.trim().toLowerCase() === 'abierto');
    if (!categoriaObj || !estadoObj) {
      alert('No se pudo encontrar la categoría o el estado.');
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
    this.ticketService.createTicket(ticketData).subscribe({
      next: ticket => {
        this.tickets.unshift(ticket);
        this.cerrarModalTicket();
        this.abrirChat(ticket);
      },
      error: err => {
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
    if (this.ticketSeleccionado) {
      this.chatService.disconnect(this.ticketSeleccionado.idTicket.toString());
    }
    this.ticketSeleccionado = ticket;
    this.mostrarChatModal = true;
    this.mensajes = [];
    // Cargar mensajes históricos
    this.chatService.getMensajesPorTicket(ticket.idTicket).subscribe(mensajes => {
      this.mensajes = mensajes.map(m => ({
        remitente: m.usuarioNombre || m.usuario, // Asegura que el nombre del usuario aparezca
        texto: m.mensajeTicket,
        esCliente: m.idUsuario === this.usuarioId
      }));
    });
    // Conecta al hub y suscríbete a los mensajes en tiempo real
    this.chatService.connect(ticket.idTicket.toString());
    this.chatService.onReceiveMessage((user, message, fecha) => {
      this.mensajes.push({
        remitente: user, // El nombre del usuario que envía el mensaje
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
    this.profileName = this.usuarioNombre;
    this.profileEmail = localStorage.getItem('usuarioEmail') || '';
    this.mostrarProfileModal = true;
  }

  closeProfileModal() {
    this.mostrarProfileModal = false;
  }

  updateProfile() {
    if (this.profileName.trim() === '') {
      this.profileName = this.usuarioNombre;
    }
    this.usuarioService.updateProfile(this.profileName, this.profileEmail).subscribe({
      next: () => {
        this.usuarioNombre = this.profileName;
        localStorage.setItem('usuario', this.profileName);
        localStorage.setItem('usuarioEmail', this.profileEmail);
        this.closeProfileModal();
        alert('Perfil actualizado.');
      },
      error: (err) => {
        console.error('Error updating profile:', err);
        alert('Error al actualizar el perfil.');
      }
    });
  }

  confirmLogout() {
    // TODO: lógica para cerrar sesión (borrar localStorage y redirigir)
    console.log('Cerrar sesión');
    localStorage.clear();
    window.location.href = '/'; // o usar el router: this.router.navigate(['/login']);
  }

  getCategoriaNombre(idCategoria: number): string {
    const categoria = this.categorias.find(c => c.idCategoria === idCategoria);
    return categoria ? categoria.nombreCategoria : 'Otro';
  }

  getEstadoNombre(idEstado: number): string {
    const estado = this.estados.find(e => e.idEstado === idEstado);
    return estado ? estado.nombreEstado : idEstado.toString();
  }

  cerrarChatModal() {
    this.mostrarChatModal = false;
    if (this.ticketSeleccionado) {
      this.chatService.disconnect(this.ticketSeleccionado.idTicket.toString());
    }
    this.ticketSeleccionado = null;
    if (this.chatSub) {
      this.chatSub.unsubscribe();
      this.chatSub = null;
    }
  }
}
