import { AfterViewChecked, Component, DestroyRef, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TicketsService, Ticket } from '../tickets/tickets.service';
import { ChatService } from '../chat/chat.service';
import { UsuarioService } from '../usuario/usuario.service';
import { AgenteService } from './agente.service';
import { AuthService } from '../core/auth.service';
import { CatalogoService } from '../core/catalogo.service';
import {
  CategoriaTicket,
  EstadoTicket,
  MensajeChatEnVivo,
  mensajeDeError,
  normalizarEstado
} from '../core/modelos';

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
  private readonly ticketsService = inject(TicketsService);
  private readonly chatService = inject(ChatService);
  private readonly usuarioService = inject(UsuarioService);
  private readonly agenteService = inject(AgenteService);
  private readonly catalogo = inject(CatalogoService);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  tickets: Ticket[] = [];
  ticketSeleccionado: Ticket | null = null;
  mensajes: MensajeChat[] = [];
  mensajeTexto = '';
  chatError = '';
  filtroBusqueda = '';
  filtroEstado = '';
  filtroPrioridad = '';
  filtroCategoria = '';
  categorias: CategoriaTicket[] = [];
  estados: EstadoTicket[] = [];
  disponible = true;
  cambiandoDisponibilidad = false;
  mostrarProfileModal = false;
  profileName = '';
  profileEmail = '';

  @ViewChild('chatScroll') chatScroll?: ElementRef<HTMLElement>;

  get usuarioNombre(): string {
    return this.auth.usuario()?.nombreUsuario ?? 'Agente';
  }

  private get usuarioId(): number {
    return this.auth.usuario()?.idUsuario ?? 0;
  }

  private get agenteId(): number | null {
    return this.auth.usuario()?.idAgente ?? null;
  }

  ngOnInit(): void {
    this.catalogo.categorias$.subscribe(categorias => this.categorias = categorias);
    this.catalogo.estados$.subscribe(estados => this.estados = estados);

    this.chatService.mensajes$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(mensaje => this.agregarMensajeEnVivo(mensaje));

    const idAgente = this.agenteId;
    if (idAgente) {
      this.agenteService.getById(idAgente).subscribe(agente => this.disponible = agente.disponibilidadAgente);
    }

    this.cargarTicketsAsignados();
  }

  ngOnDestroy(): void {
    void this.chatService.desconectar();
  }

  ngAfterViewChecked(): void {
    const contenedor = this.chatScroll?.nativeElement;
    if (contenedor) {
      contenedor.scrollTop = contenedor.scrollHeight;
    }
  }

  // ---------- Tickets ----------

  cargarTicketsAsignados(): void {
    const idAgente = this.agenteId;
    if (!idAgente) {
      return;
    }

    this.ticketsService.getTicketsByAgente(idAgente).subscribe(tickets => {
      this.tickets = tickets;

      // Mantiene el ticket abierto actualizado tras recargar la lista.
      if (this.ticketSeleccionado) {
        this.ticketSeleccionado =
          tickets.find(t => t.idTicket === this.ticketSeleccionado!.idTicket) ?? this.ticketSeleccionado;
      }
    });
  }

  get totalAbiertos(): number {
    return this.tickets.filter(t => normalizarEstado(t.nombreEstado) === 'abierto').length;
  }

  get totalEnProgreso(): number {
    return this.tickets.filter(t => normalizarEstado(t.nombreEstado) === 'en-progreso').length;
  }

  get totalUrgentes(): number {
    return this.tickets.filter(t => t.prioridadTicket === 'urgente').length;
  }

  getCategoriaNombre(idCategoria: number): string {
    return this.categorias.find(c => c.idCategoria === idCategoria)?.nombreCategoria ?? 'Otro';
  }

  cambiarEstadoTicket(ticket: Ticket, estado: string): void {
    const estadoObj = this.estados.find(e => normalizarEstado(e.nombreEstado) === normalizarEstado(estado));
    if (!estadoObj) {
      alert(`El estado "${estado}" no existe en el catálogo.`);
      return;
    }

    this.guardarCambio(ticket, { idEstadoTicket: estadoObj.idEstado });
  }

  cambiarPrioridad(ticket: Ticket, nuevaPrioridad: string): void {
    this.guardarCambio(ticket, { prioridadTicket: nuevaPrioridad });
  }

  /** "Urgente" es una prioridad, no un estado (antes este botón no hacía nada). */
  ponerUrgente(ticket: Ticket): void {
    this.cambiarPrioridad(ticket, 'urgente');
  }

  private guardarCambio(ticket: Ticket, cambios: Parameters<typeof TicketsService.cambioDesde>[1]): void {
    this.ticketsService.update(ticket.idTicket, TicketsService.cambioDesde(ticket, cambios)).subscribe({
      next: () => this.cargarTicketsAsignados(),
      error: err => alert(mensajeDeError(err, 'No se pudo actualizar el ticket.'))
    });
  }

  // ---------- Disponibilidad ----------

  /** El agente decide si entra en el reparto automático de tickets nuevos. */
  alternarDisponibilidad(): void {
    const idAgente = this.agenteId;
    if (!idAgente || this.cambiandoDisponibilidad) {
      return;
    }

    const nuevoValor = !this.disponible;
    this.cambiandoDisponibilidad = true;

    this.agenteService.cambiarDisponibilidad(idAgente, nuevoValor).subscribe({
      next: () => {
        this.disponible = nuevoValor;
        this.cambiandoDisponibilidad = false;
      },
      error: err => {
        this.cambiandoDisponibilidad = false;
        alert(mensajeDeError(err, 'No se pudo cambiar la disponibilidad.'));
      }
    });
  }

  // ---------- Chat ----------

  abrirChat(ticket: Ticket): void {
    this.ticketSeleccionado = ticket;
    this.mensajes = [];
    this.chatError = '';

    // Al abrir un ticket todavía no empezado, pasa a "en progreso".
    if (normalizarEstado(ticket.nombreEstado) === 'abierto') {
      this.cambiarEstadoTicket(ticket, 'en-progreso');
    }

    this.chatService.historial(ticket.idTicket).subscribe({
      next: historial => {
        this.mensajes = historial.map(m => ({
          remitente: m.usuarioNombre,
          texto: m.mensajeTicket,
          esAgente: m.idUsuario === this.usuarioId
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

  private agregarMensajeEnVivo(mensaje: MensajeChatEnVivo): void {
    if (mensaje.idTicket !== this.ticketSeleccionado?.idTicket) {
      return;
    }

    this.mensajes.push({
      remitente: mensaje.usuario,
      texto: mensaje.mensaje,
      esAgente: mensaje.idUsuario === this.usuarioId
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
      next: () => this.closeProfileModal(),
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
