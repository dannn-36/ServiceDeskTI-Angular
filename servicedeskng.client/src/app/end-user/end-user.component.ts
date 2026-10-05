import { Component, DestroyRef, OnDestroy, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TicketsService, Ticket } from '../tickets/tickets.service';
import { ChatService } from '../chat/chat.service';
import { UsuarioService } from '../usuario/usuario.service';
import { AuthService } from '../core/auth.service';
import { CatalogoService } from '../core/catalogo.service';
import { CategoriaTicket, EstadoTicket, MensajeChatEnVivo, mensajeDeError, normalizarEstado } from '../core/modelos';

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
  private readonly ticketService = inject(TicketsService);
  private readonly chatService = inject(ChatService);
  private readonly usuarioService = inject(UsuarioService);
  private readonly catalogo = inject(CatalogoService);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  tickets: Ticket[] = [];
  nuevoTicket = { asunto: '', descripcion: '', categoria: '' };
  mostrarModalTicket = false;
  creandoTicket = false;
  ticketSeleccionado: Ticket | null = null;
  mensajes: MensajeChat[] = [];
  mensajeTexto = '';
  chatError = '';
  categoriaSeleccionada = '';

  categorias: CategoriaTicket[] = [];
  estados: EstadoTicket[] = [];

  filtroEstado = '';
  ticketsFiltrados: Ticket[] = [];

  mostrarChatModal = false;

  mostrarProfileModal = false;
  profileName = '';
  profileEmail = '';

  get usuarioNombre(): string {
    return this.auth.usuario()?.nombreUsuario ?? 'Cliente';
  }

  private get usuarioId(): number {
    return this.auth.usuario()?.idUsuario ?? 0;
  }

  ngOnInit(): void {
    this.catalogo.categorias$.subscribe(categorias => this.categorias = categorias);
    this.catalogo.estados$.subscribe(estados => this.estados = estados);

    this.chatService.mensajes$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(mensaje => this.agregarMensajeEnVivo(mensaje));

    this.cargarTickets();
  }

  ngOnDestroy(): void {
    void this.chatService.desconectar();
  }

  // ---------- Tickets ----------

  cargarTickets(): void {
    const idCliente = this.auth.usuario()?.idCliente;
    if (!idCliente) {
      return;
    }

    this.ticketService.getTicketsByUser(idCliente).subscribe(tickets => {
      this.tickets = tickets;
      this.filtrarTickets();
    });
  }

  filtrarTickets(): void {
    this.ticketsFiltrados = this.filtroEstado
      ? this.tickets.filter(t => normalizarEstado(this.getEstadoNombre(t.idEstadoTicket))
          === normalizarEstado(this.filtroEstado))
      : this.tickets;
  }

  abrirModalTicket(categoria = ''): void {
    this.mostrarModalTicket = true;
    this.categoriaSeleccionada = categoria;

    // Los accesos rápidos usan nombres en minúscula; se busca la categoría real.
    const coincidencia = this.categorias.find(
      c => c.nombreCategoria.trim().toLowerCase() === categoria.trim().toLowerCase());
    if (coincidencia) {
      this.nuevoTicket.categoria = coincidencia.nombreCategoria;
    }
  }

  cerrarModalTicket(): void {
    this.mostrarModalTicket = false;
    this.categoriaSeleccionada = '';
    this.nuevoTicket = { asunto: '', descripcion: '', categoria: '' };
  }

  crearTicket(): void {
    const categoria = this.categorias.find(
      c => c.nombreCategoria.trim().toLowerCase() === this.nuevoTicket.categoria.trim().toLowerCase());

    if (!categoria) {
      alert('Debes seleccionar una categoría.');
      return;
    }

    this.creandoTicket = true;

    // El cliente no indica su id: el servidor lo toma de la sesión.
    this.ticketService.createTicket({
      idCategoriaTicket: categoria.idCategoria,
      tituloTicket: this.nuevoTicket.asunto,
      descripcionTicket: this.nuevoTicket.descripcion,
      prioridadTicket: 'media'
    }).subscribe({
      next: ticket => {
        this.creandoTicket = false;
        this.tickets.unshift(ticket);
        this.filtrarTickets();
        this.cerrarModalTicket();
        this.abrirChat(ticket);
      },
      error: err => {
        this.creandoTicket = false;
        alert(mensajeDeError(err, 'No se pudo crear el ticket.'));
      }
    });
  }

  getNombreEstado(idEstado: number): string {
    return this.getEstadoNombre(idEstado);
  }

  getNombreCategoria(idCategoria: number): string {
    return this.getCategoriaNombre(idCategoria);
  }

  getCategoriaNombre(idCategoria: number): string {
    return this.categorias.find(c => c.idCategoria === idCategoria)?.nombreCategoria ?? 'Otro';
  }

  getEstadoNombre(idEstado: number): string {
    return this.estados.find(e => e.idEstado === idEstado)?.nombreEstado ?? String(idEstado);
  }

  // ---------- Chat ----------

  abrirChat(ticket: Ticket): void {
    this.ticketSeleccionado = ticket;
    this.mostrarChatModal = true;
    this.mensajes = [];
    this.chatError = '';

    this.chatService.historial(ticket.idTicket).subscribe({
      next: historial => {
        this.mensajes = historial.map(m => ({
          remitente: m.usuarioNombre,
          texto: m.mensajeTicket,
          esCliente: m.idUsuario === this.usuarioId
        }));
      },
      error: err => this.chatError = mensajeDeError(err, 'No se pudo cargar el historial.')
    });

    this.chatService.conectar(ticket.idTicket)
      .catch(() => this.chatError = 'No se pudo conectar al chat en tiempo real.');
  }

  enviarMensaje(): void {
    const texto = this.mensajeTexto.trim();
    if (!texto || !this.ticketSeleccionado) {
      return;
    }

    this.mensajeTexto = '';
    this.chatService.enviar(texto).catch(err => {
      this.mensajeTexto = texto;
      this.chatError = err?.message ?? 'No se pudo enviar el mensaje.';
    });
  }

  cerrarChatModal(): void {
    this.mostrarChatModal = false;
    this.ticketSeleccionado = null;
    void this.chatService.desconectar();
  }

  private agregarMensajeEnVivo(mensaje: MensajeChatEnVivo): void {
    if (mensaje.idTicket !== this.ticketSeleccionado?.idTicket) {
      return;
    }

    this.mensajes.push({
      remitente: mensaje.usuario,
      texto: mensaje.mensaje,
      esCliente: mensaje.idUsuario === this.usuarioId
    });
  }

  // ---------- Perfil y sesión ----------

  showProfileModal(): void {
    this.profileName = this.usuarioNombre;
    this.profileEmail = this.auth.usuario()?.correoUsuario ?? '';
    this.mostrarProfileModal = true;
  }

  closeProfileModal(): void {
    this.mostrarProfileModal = false;
  }

  updateProfile(): void {
    const nombre = this.profileName.trim() || this.usuarioNombre;

    this.usuarioService.updateProfile(nombre, this.profileEmail.trim()).subscribe({
      next: () => {
        this.closeProfileModal();
        alert('Perfil actualizado.');
      },
      error: err => alert(mensajeDeError(err, 'No se pudo actualizar el perfil.'))
    });
  }

  confirmLogout(): void {
    if (confirm('¿Estás seguro de que quieres cerrar sesión?')) {
      void this.chatService.desconectar();
      this.auth.logout();
    }
  }
}
