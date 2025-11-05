import { Component, OnInit, OnDestroy, AfterViewChecked, ViewChild, ElementRef } from '@angular/core';
import { TicketsService, Ticket } from '../tickets/tickets.service';
import { ChatService } from '../chat/chat.service';
import { HttpClient } from '@angular/common/http';
import { UsuarioService } from '../usuario/usuario.service';

interface MensajeChat {
  remitente: string;
  texto: string;
  esAgente: boolean;
}

interface EstadoTicket {
  idEstado: number;
  nombreEstado: string;
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
  filtroBusqueda: string = '';
  filtroEstado: string = '';
  filtroPrioridad: string = '';
  filtroCategoria: string = '';
  categorias: any[] = [];
  estados: EstadoTicket[] = [];
  mostrarProfileModal: boolean = false;
  profileName: string = '';
  profileEmail: string = '';
  @ViewChild('chatScroll') chatScroll!: ElementRef;

  constructor(
    private ticketsService: TicketsService,
    private chatService: ChatService,
    private http: HttpClient,
    private usuarioService: UsuarioService
  ) {}

  ngOnInit() {
    this.usuarioNombre = localStorage.getItem('usuario') || 'Agente';
    this.usuarioId = +(localStorage.getItem('usuarioId') || 0);
    this.cargarCategorias();
    this.cargarEstados();
    this.cargarTicketsAsignados();
  }

  cargarCategorias() {
    this.http.get<any[]>('/api/tickets/categorias').subscribe(data => {
      this.categorias = data;
    });
  }

  cargarEstados() {
    this.http.get<EstadoTicket[]>('/api/tickets/estados').subscribe(estados => {
      this.estados = estados;
    });
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
    // Cambiar estado a "En Progreso" si no lo está
    const estadoEnProgreso = this.estados.find(e => e.nombreEstado.toLowerCase() === 'en-progreso');
    if (estadoEnProgreso && ticket.idEstadoTicket !== estadoEnProgreso.idEstado) {
      const actualizado = { ...ticket, idEstadoTicket: estadoEnProgreso.idEstado };
      this.ticketsService.update(ticket.idTicket, actualizado).subscribe(() => {
        ticket.idEstadoTicket = estadoEnProgreso.idEstado;
      });
    }
    // Cargar mensajes históricos
    this.chatService.getMensajesPorTicket(ticket.idTicket).subscribe(mensajes => {
      this.mensajes = mensajes.map(m => ({
        remitente: m.usuarioNombre || m.nombreUsuario || m.usuario, // Siempre usar el nombre guardado
        texto: m.mensajeTicket,
        esAgente: m.idUsuario === this.usuarioId
      }));
    });
    // Conecta al hub y suscríbete a los mensajes en tiempo real
    this.chatService.connect(ticket.idTicket.toString());
    this.chatService.onReceiveMessage((user, message, fecha) => {
      this.mensajes.push({
        remitente: user, // Usar siempre el nombre recibido
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
    this.mensajeTexto = '';
  }

  cambiarEstadoTicket(ticket: Ticket, estado: string) {
    const estadoObj = this.estados.find(e => e.nombreEstado.toLowerCase() === estado.toLowerCase());
    if (!estadoObj) return;
    // Si el estado es 'Urgente', también cambia la prioridad a 'urgente'
    // Si el estado es 'En Progreso', mantiene la prioridad actual
    let nuevaPrioridad = ticket.prioridadTicket || 'media';
    if (estado.toLowerCase() === 'urgente') {
      nuevaPrioridad = 'urgente';
    }
    const actualizado = {
      ...ticket,
      idEstadoTicket: estadoObj.idEstado,
      tituloTicket: ticket.tituloTicket || '',
      descripcionTicket: ticket.descripcionTicket || '',
      idCliente: ticket.idCliente,
      idCategoriaTicket: ticket.idCategoriaTicket,
      prioridadTicket: nuevaPrioridad,
      ubicacionTicket: ticket.ubicacionTicket || '',
      departamentoTicket: ticket.departamentoTicket || '',
      fechaHoraCreacionTicket: ticket.fechaHoraCreacionTicket || '',
      fechaHoraActualizacionTicket: new Date().toISOString()
    };
    this.ticketsService.update(ticket.idTicket, actualizado).subscribe({
      next: () => {
        this.cargarTicketsAsignados(); // Recarga la lista de tickets
        // Actualiza el ticket seleccionado si corresponde
        if (this.ticketSeleccionado && this.ticketSeleccionado.idTicket === ticket.idTicket) {
          this.ticketSeleccionado.idEstadoTicket = estadoObj.idEstado;
          this.ticketSeleccionado.prioridadTicket = nuevaPrioridad;
        }
      },
      error: (err) => { /* Manejo de error */ }
    });
  }

  cambiarPrioridad(ticket: Ticket, nuevaPrioridad: string) {
    const actualizado = {
      ...ticket,
      prioridadTicket: nuevaPrioridad,
      idEstadoTicket: ticket.idEstadoTicket,
      tituloTicket: ticket.tituloTicket || '',
      descripcionTicket: ticket.descripcionTicket || '',
      idCliente: ticket.idCliente,
      idCategoriaTicket: ticket.idCategoriaTicket,
      ubicacionTicket: ticket.ubicacionTicket || '',
      departamentoTicket: ticket.departamentoTicket || '',
      fechaHoraCreacionTicket: ticket.fechaHoraCreacionTicket || '',
      fechaHoraActualizacionTicket: new Date().toISOString()
    };
    this.ticketsService.update(ticket.idTicket, actualizado).subscribe({
      next: () => {
        this.cargarTicketsAsignados();
        if (this.ticketSeleccionado && this.ticketSeleccionado.idTicket === ticket.idTicket) {
          this.ticketSeleccionado.prioridadTicket = nuevaPrioridad;
        }
      },
      error: (err) => { /* Manejo de error */ }
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

  getTicketsCountByEstado(estado: number): number {
    return this.tickets.filter(t => t.idEstadoTicket === estado).length;
  }

  logout() {
    localStorage.clear();
    window.location.href = '/';
  }

  confirmLogout() {
    if (confirm('¿Estás seguro de que quieres cerrar sesión?')) {
      this.logout();
    }
  }

  ponerUrgente(ticket: Ticket) {
    const actualizado = {
      ...ticket,
      prioridadTicket: 'urgente',
      idEstadoTicket: ticket.idEstadoTicket,
      tituloTicket: ticket.tituloTicket || '',
      descripcionTicket: ticket.descripcionTicket || '',
      idCliente: ticket.idCliente,
      idCategoriaTicket: ticket.idCategoriaTicket,
      ubicacionTicket: ticket.ubicacionTicket || '',
      departamentoTicket: ticket.departamentoTicket || '',
      fechaHoraCreacionTicket: ticket.fechaHoraCreacionTicket || '',
      fechaHoraActualizacionTicket: new Date().toISOString()
    };
    this.ticketsService.update(ticket.idTicket, actualizado).subscribe({
      next: () => {
        this.cargarTicketsAsignados(); // Recarga la lista
        if (this.ticketSeleccionado && this.ticketSeleccionado.idTicket === ticket.idTicket) {
          this.ticketSeleccionado.prioridadTicket = 'urgente';
        }
      },
      error: (err) => { /* Manejo de error */ }
    });
  }

  getCategoriaNombre(idCategoria: number): string {
    const categoria = this.categorias.find(c => c.idCategoria === idCategoria);
    return categoria ? categoria.nombreCategoria : 'Otro';
  }

  crearTicket(ticketData: { categoria: string, estado: string, asunto: string, descripcion: string }) {
    console.log('Categorias cargadas:', this.categorias);
    console.log('Valor seleccionado:', ticketData.categoria);
    // Buscar el ID de la categoría seleccionada (normalizado)
    const categoriaObj = this.categorias.find(
      c => c.nombreCategoria.trim().toLowerCase() === ticketData.categoria.trim().toLowerCase()
    );
    // Buscar el ID del estado
    const estadoObj = this.estados.find(e => e.nombreEstado.trim().toLowerCase() === ticketData.estado.trim().toLowerCase());
    if (!categoriaObj || !estadoObj) {
      alert('No se pudo encontrar la categoría o el estado.');
      return;
    }
    const ticket = {
      idCategoriaTicket: categoriaObj.idCategoria,
      idEstadoTicket: estadoObj.idEstado,
      tituloTicket: ticketData.asunto,
      descripcionTicket: ticketData.descripcion,
      prioridadTicket: 'media',
      ubicacionTicket: '',
      departamentoTicket: ''
      // ...otros campos necesarios
    };
    this.ticketsService.createTicket(ticket).subscribe({
      next: () => {
        this.cargarTicketsAsignados();
      },
      error: err => {
        alert('Error al crear ticket: ' + JSON.stringify(err.error?.errors || err.error));
      }
    });
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
      },
      error: (err) => {
        console.error('Error updating profile:', err);
      }
    });
  }

  ngOnDestroy(): void {
    // Limpieza de recursos si es necesario
  }
}
