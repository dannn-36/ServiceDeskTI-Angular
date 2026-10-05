import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import Chart from 'chart.js/auto';
import { UsuarioService, Usuario } from '../usuario/usuario.service';
import { TicketsService, Ticket } from '../tickets/tickets.service';
import { AuthService } from '../core/auth.service';
import { CatalogoService } from '../core/catalogo.service';
import { RespaldoService } from '../core/respaldo.service';
import {
  CategoriaTicket,
  EstadoTicket,
  descargarArchivo,
  mensajeDeError,
  mensajeDeErrorBlob,
  normalizarEstado
} from '../core/modelos';

type Seccion = 'dashboard' | 'users' | 'tickets' | 'reports' | 'settings' | 'audit';

@Component({
  selector: 'app-administrador',
  templateUrl: './administrador.component.html',
  styleUrls: ['./administrador.component.css']
})
export class AdministradorComponent implements OnInit, OnDestroy {
  private readonly usuarioService = inject(UsuarioService);
  private readonly ticketsService = inject(TicketsService);
  private readonly catalogo = inject(CatalogoService);
  private readonly respaldo = inject(RespaldoService);
  private readonly auth = inject(AuthService);
  private readonly http = inject(HttpClient);

  // ---------- Usuarios ----------
  users: Usuario[] = [];
  filteredUsers: Usuario[] = [];
  userSearch = '';
  userRoleFilter = '';
  showUserModal = false;
  loadingUsers = false;
  userError = '';
  selectedUser: Usuario | null = null;
  formUser: Partial<Usuario> = { tipoUsuario: 'Cliente' };
  userEditMode = false;

  totalClientes = 0;
  totalAgentes = 0;
  totalSupervisores = 0;
  totalAdministradores = 0;

  // ---------- Secciones y perfil ----------
  currentSection: Seccion = 'dashboard';
  showProfile = false;
  profileName = '';
  profileEmail = '';

  // ---------- Tickets ----------
  tickets: Ticket[] = [];
  filteredTickets: Ticket[] = [];
  ticketSearch = '';
  ticketStatusFilter = '';
  ticketCategoryFilter = '';
  loadingTickets = false;
  ticketError = '';
  estados: EstadoTicket[] = [];
  categorias: CategoriaTicket[] = [];

  totalAbiertos = 0;
  totalEnProgreso = 0;
  totalPendientes = 0;
  totalResueltos = 0;
  totalUrgentes = 0;

  // ---------- Configuración ----------
  slaHoras: number | null = null;
  restaurarArchivo: File | null = null;
  restaurarMensaje = '';
  backupCargando = false;

  private graficas: Record<string, Chart | undefined> = {};

  get usuarioNombre(): string {
    return this.auth.usuario()?.nombreUsuario ?? 'Administrador';
  }

  ngOnInit(): void {
    this.getUsers();
    this.getTickets();
    this.http.get<{ horasVencimiento: number }>('/api/tickets/sla')
      .subscribe({ next: sla => this.slaHoras = sla.horasVencimiento, error: () => this.slaHoras = null });
  }

  ngOnDestroy(): void {
    Object.values(this.graficas).forEach(grafica => grafica?.destroy());
  }

  showSection(section: Seccion): void {
    this.currentSection = section;

    if (section === 'dashboard') {
      this.getTickets();
    } else if (section === 'reports') {
      setTimeout(() => this.initReportCharts());
    } else if (section === 'users') {
      this.filterUsers();
    } else if (section === 'tickets') {
      this.filterTickets();
    }
  }

  // ======================================================
  // Usuarios
  // ======================================================

  getUsers(): void {
    this.loadingUsers = true;
    this.userError = '';

    this.usuarioService.getAll().subscribe({
      next: users => {
        this.users = users;
        this.filterUsers();
        this.loadingUsers = false;

        const contar = (rol: string) => users.filter(u => u.tipoUsuario.toLowerCase() === rol).length;
        this.totalClientes = contar('cliente');
        this.totalAgentes = contar('agente');
        this.totalSupervisores = contar('supervisor');
        this.totalAdministradores = contar('administrador');
      },
      error: err => {
        this.userError = mensajeDeError(err, 'Error al cargar usuarios.');
        this.loadingUsers = false;
      }
    });
  }

  filterUsers(): void {
    const busqueda = this.userSearch.toLowerCase();
    const rol = this.userRoleFilter.toLowerCase();

    this.filteredUsers = this.users.filter(user => {
      const coincideBusqueda = user.nombreUsuario.toLowerCase().includes(busqueda)
        || user.correoUsuario.toLowerCase().includes(busqueda);
      const coincideRol = !rol || user.tipoUsuario.toLowerCase() === rol;
      return coincideBusqueda && coincideRol;
    });
  }

  openUserModal(mode: 'new' | 'edit' = 'new'): void {
    this.showUserModal = true;
    this.userError = '';
    if (mode === 'new') {
      this.userEditMode = false;
      this.selectedUser = null;
      this.formUser = { tipoUsuario: 'Cliente' };
    }
  }

  closeUserModal(): void {
    this.showUserModal = false;
    this.userEditMode = false;
    this.selectedUser = null;
    this.formUser = { tipoUsuario: 'Cliente' };
  }

  startEditUser(user: Usuario): void {
    this.selectedUser = user;
    this.formUser = { ...user, contrasenaUsuario: '' };
    this.userEditMode = true;
    this.openUserModal('edit');
  }

  /** Si el usuario tiene historial, el servidor lo desactiva en lugar de borrarlo y lo explica. */
  deleteUser(user: Usuario): void {
    if (!confirm(`¿Dar de baja a ${user.nombreUsuario}?`)) {
      return;
    }

    this.loadingUsers = true;
    this.usuarioService.delete(user.idUsuario!).subscribe({
      next: resultado => {
        if (!resultado.eliminado) {
          alert(resultado.message);
        }
        this.getUsers();
      },
      error: err => {
        this.loadingUsers = false;
        alert(mensajeDeError(err, 'Error al eliminar el usuario.'));
      }
    });
  }

  saveUserEdit(): void {
    if (!this.selectedUser?.idUsuario) {
      return;
    }

    this.loadingUsers = true;
    this.usuarioService.update(this.selectedUser.idUsuario, {
      nombreUsuario: this.formUser.nombreUsuario ?? '',
      correoUsuario: this.formUser.correoUsuario ?? '',
      contrasenaUsuario: this.formUser.contrasenaUsuario || undefined,
      departamentoUsuario: this.formUser.departamentoUsuario,
      ubicacionUsuario: this.formUser.ubicacionUsuario,
      estadoUsuario: this.formUser.estadoUsuario
    }).subscribe({
      next: () => {
        this.closeUserModal();
        this.getUsers();
      },
      error: err => {
        this.loadingUsers = false;
        this.userError = mensajeDeError(err, 'Error al editar el usuario.');
      }
    });
  }

  createUser(): void {
    this.loadingUsers = true;

    this.usuarioService.create({
      nombreUsuario: this.formUser.nombreUsuario ?? '',
      correoUsuario: this.formUser.correoUsuario ?? '',
      contrasenaUsuario: this.formUser.contrasenaUsuario ?? '',
      tipoUsuario: this.formUser.tipoUsuario ?? 'Cliente',
      departamentoUsuario: this.formUser.departamentoUsuario ?? '',
      estadoUsuario: 'activo'
    }).subscribe({
      next: () => {
        this.closeUserModal();
        this.getUsers();
      },
      error: err => {
        this.loadingUsers = false;
        // El servidor explica qué falla (correo repetido, contraseña corta, etc.).
        this.userError = mensajeDeError(err, 'Error al crear el usuario.');
      }
    });
  }

  onRoleChange(event: Event): void {
    this.formUser.tipoUsuario = (event.target as HTMLSelectElement).value;
  }

  // ======================================================
  // Tickets
  // ======================================================

  /** Catálogos y tickets en paralelo; ya no hace falta esperar 200 ms "por si acaso". */
  getTickets(): void {
    this.loadingTickets = true;
    this.ticketError = '';

    forkJoin({
      estados: this.catalogo.estados$,
      categorias: this.catalogo.categorias$,
      tickets: this.ticketsService.getAll()
    }).subscribe({
      next: ({ estados, categorias, tickets }) => {
        this.estados = estados;
        this.categorias = categorias;
        this.tickets = tickets;
        this.updateTicketCounts();
        this.filterTickets();
        this.loadingTickets = false;
        setTimeout(() => this.initCharts());
      },
      error: err => {
        this.ticketError = mensajeDeError(err, 'Error al cargar tickets.');
        this.loadingTickets = false;
      }
    });
  }

  updateTicketCounts(): void {
    const contar = (estado: string) =>
      this.tickets.filter(t => normalizarEstado(this.getEstadoNombre(t.idEstadoTicket)) === estado).length;

    this.totalAbiertos = contar('abierto');
    this.totalEnProgreso = contar('en-progreso');
    this.totalPendientes = contar('pendiente');
    this.totalResueltos = contar('resuelto');
    this.totalUrgentes = this.tickets.filter(t => t.prioridadTicket?.toLowerCase() === 'urgente').length;
  }

  filterTickets(): void {
    const busqueda = this.ticketSearch.toLowerCase();
    const estado = this.ticketStatusFilter.toLowerCase();
    const categoria = this.ticketCategoryFilter.toLowerCase();

    this.filteredTickets = this.tickets.filter(ticket => {
      const coincideBusqueda = ticket.tituloTicket.toLowerCase().includes(busqueda)
        || (ticket.descripcionTicket ?? '').toLowerCase().includes(busqueda)
        || ticket.idTicket.toString().includes(busqueda);

      // "Urgente" es una prioridad; el resto de opciones son estados.
      const coincideEstado = !estado
        || (estado === 'urgente'
          ? ticket.prioridadTicket?.toLowerCase() === 'urgente'
          : normalizarEstado(this.getEstadoNombre(ticket.idEstadoTicket)) === normalizarEstado(estado));

      const coincideCategoria = !categoria
        || this.getCategoriaNombre(ticket.idCategoriaTicket).toLowerCase() === categoria;

      return coincideBusqueda && coincideEstado && coincideCategoria;
    });
  }

  getEstadoNombre(idEstado: number): string {
    const estado = this.estados.find(e => e.idEstado === idEstado);
    if (!estado) {
      return String(idEstado);
    }

    switch (normalizarEstado(estado.nombreEstado)) {
      case 'abierto': return 'Abierto';
      case 'en-progreso': return 'En Progreso';
      case 'pendiente': return 'Pendiente';
      case 'resuelto': return 'Resuelto';
      default: return estado.nombreEstado;
    }
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

    this.guardarCambioTicket(ticket, { idEstadoTicket: estadoObj.idEstado });
  }

  finalizarTicket(ticket: Ticket): void {
    this.cambiarEstadoTicket(ticket, 'Resuelto');
  }

  ponerPendiente(ticket: Ticket): void {
    this.cambiarEstadoTicket(ticket, 'Pendiente');
  }

  ponerEnProgreso(ticket: Ticket): void {
    this.cambiarEstadoTicket(ticket, 'En Progreso');
  }

  ponerUrgente(ticket: Ticket): void {
    this.guardarCambioTicket(ticket, { prioridadTicket: 'urgente' });
  }

  private guardarCambioTicket(ticket: Ticket, cambios: Parameters<typeof TicketsService.cambioDesde>[1]): void {
    this.ticketsService.update(ticket.idTicket, TicketsService.cambioDesde(ticket, cambios)).subscribe({
      next: () => this.getTickets(),
      error: err => alert(mensajeDeError(err, 'No se pudo actualizar el ticket.'))
    });
  }

  getRecentTickets(): Ticket[] {
    return [...this.tickets]
      .sort((a, b) =>
        new Date(b.fechaHoraCreacionTicket ?? 0).getTime() - new Date(a.fechaHoraCreacionTicket ?? 0).getTime())
      .slice(0, 3);
  }

  // ======================================================
  // Gráficas
  // ======================================================

  /** Tickets creados y resueltos por mes del año en curso. */
  getMonthlyTicketStats(): { labels: string[]; created: number[]; resolved: number[] } {
    const meses = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
    const creados: number[] = Array(12).fill(0);
    const resueltos: number[] = Array(12).fill(0);
    const anioActual = new Date().getFullYear();

    for (const ticket of this.tickets) {
      if (!ticket.fechaHoraCreacionTicket) {
        continue;
      }
      const fecha = new Date(ticket.fechaHoraCreacionTicket);
      if (fecha.getFullYear() !== anioActual) {
        continue;
      }
      creados[fecha.getMonth()]++;
      if (this.getEstadoNombre(ticket.idEstadoTicket) === 'Resuelto') {
        resueltos[fecha.getMonth()]++;
      }
    }

    return { labels: meses, created: creados, resolved: resueltos };
  }

  initCharts(): void {
    this.dibujar('statusChart', canvas => new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels: ['Abiertos', 'En Progreso', 'Pendientes', 'Resueltos'],
        datasets: [{
          data: [this.totalAbiertos, this.totalEnProgreso, this.totalPendientes, this.totalResueltos],
          backgroundColor: ['#3b82f6', '#f59e0b', '#8b5cf6', '#10b981']
        }]
      },
      options: { responsive: true, maintainAspectRatio: false }
    }));

    const mensual = this.getMonthlyTicketStats();
    this.dibujar('trendChart', canvas => new Chart(canvas, {
      type: 'line',
      data: {
        labels: mensual.labels,
        datasets: [
          { label: 'Tickets Creados', data: mensual.created, borderColor: '#3b82f6', backgroundColor: 'rgba(59,130,246,0.1)', tension: 0.4 },
          { label: 'Tickets Resueltos', data: mensual.resolved, borderColor: '#10b981', backgroundColor: 'rgba(16,185,129,0.1)', tension: 0.4 }
        ]
      },
      options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
    }));
  }

  initReportCharts(): void {
    // Resueltos por agente, con su nombre (antes la etiqueta era "Agente 7").
    const porAgente = new Map<string, number>();
    for (const ticket of this.tickets) {
      if (ticket.nombreAgente && this.getEstadoNombre(ticket.idEstadoTicket) === 'Resuelto') {
        porAgente.set(ticket.nombreAgente, (porAgente.get(ticket.nombreAgente) ?? 0) + 1);
      }
    }

    this.dibujar('agentPerformanceChart', canvas => new Chart(canvas, {
      type: 'bar',
      data: {
        labels: [...porAgente.keys()],
        datasets: [{ label: 'Tickets Resueltos', data: [...porAgente.values()], backgroundColor: '#3b82f6' }]
      },
      options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
    }));

    this.dibujar('categoryChart', canvas => new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels: this.categorias.map(c => c.nombreCategoria),
        datasets: [{
          data: this.categorias.map(c => this.tickets.filter(t => t.idCategoriaTicket === c.idCategoria).length),
          backgroundColor: ['#ef4444', '#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#14b8a6', '#64748b']
        }]
      },
      options: { responsive: true, maintainAspectRatio: false }
    }));
  }

  private dibujar(id: string, crear: (canvas: HTMLCanvasElement) => Chart): void {
    const canvas = document.getElementById(id) as HTMLCanvasElement | null;
    this.graficas[id]?.destroy();
    this.graficas[id] = canvas ? crear(canvas) : undefined;
  }

  // ======================================================
  // Respaldo y restauración
  // ======================================================

  crearRespaldo(): void {
    this.backupCargando = true;

    this.respaldo.descargarRespaldo().subscribe({
      next: blob => {
        descargarArchivo(blob, `backup_servicedesk_${new Date().toISOString().slice(0, 10)}.sql`);
        this.backupCargando = false;
      },
      error: async err => {
        this.backupCargando = false;
        alert(await mensajeDeErrorBlob(err, 'Error al crear el respaldo.'));
      }
    });
  }

  onArchivoSeleccionado(event: Event): void {
    const archivo = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.restaurarArchivo = archivo;
    this.restaurarMensaje = archivo ? archivo.name : 'Sin archivos seleccionados';
  }

  restaurarRespaldo(): void {
    if (!this.restaurarArchivo) {
      alert('Selecciona un archivo de respaldo.');
      return;
    }

    // Restaurar sobrescribe la base de datos completa: se pide confirmación explícita.
    if (!confirm(`Se sobrescribirá la base de datos con "${this.restaurarArchivo.name}". ¿Continuar?`)) {
      return;
    }

    this.backupCargando = true;

    this.respaldo.restaurarRespaldo(this.restaurarArchivo).subscribe({
      next: evento => {
        if (evento.type === HttpEventType.Response) {
          this.backupCargando = false;
          alert('Restauración completada correctamente.');
        }
      },
      error: err => {
        this.backupCargando = false;
        alert(mensajeDeError(err, 'Error al restaurar el respaldo.'));
      }
    });
  }

  // ======================================================
  // Perfil y sesión
  // ======================================================

  openProfile(): void {
    this.profileName = this.usuarioNombre;
    this.profileEmail = this.auth.usuario()?.correoUsuario ?? '';
    this.showProfile = true;
  }

  closeProfile(): void {
    this.showProfile = false;
  }

  /** Antes el botón "Guardar" solo cerraba el modal. */
  guardarPerfil(): void {
    const nombre = this.profileName.trim() || this.usuarioNombre;

    this.usuarioService.updateProfile(nombre, this.profileEmail.trim()).subscribe({
      next: () => {
        this.closeProfile();
        this.getUsers();
      },
      error: err => alert(mensajeDeError(err, 'No se pudo actualizar el perfil.'))
    });
  }

  logout(): void {
    this.auth.logout();
  }
}
