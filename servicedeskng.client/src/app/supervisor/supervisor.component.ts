import { AfterViewInit, Component, DestroyRef, OnDestroy, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin } from 'rxjs';
import Chart from 'chart.js/auto';
import { SupervisorService, TeamMember, Ticket, Escalation } from './supervisor.service';
import { ChatService } from '../chat/chat.service';
import { UsuarioService } from '../usuario/usuario.service';
import { AuthService } from '../core/auth.service';
import { CatalogoService } from '../core/catalogo.service';
import {
  CategoriaTicket,
  ComparativaAgente,
  MensajeChatEnVivo,
  RendimientoSemanal,
  descargarArchivo,
  esEstadoActivo,
  esEstadoFinalizado,
  mensajeDeError,
  mensajeDeErrorBlob,
  normalizarEstado
} from '../core/modelos';

interface MensajeSupervision {
  remitente: string;
  texto: string;
  esSupervisor: boolean;
}

@Component({
  selector: 'app-supervisor',
  templateUrl: './supervisor.component.html',
  styleUrls: ['./supervisor.component.css']
})
export class SupervisorComponent implements AfterViewInit, OnInit, OnDestroy {
  private readonly supervisorService = inject(SupervisorService);
  private readonly chatService = inject(ChatService);
  private readonly usuarioService = inject(UsuarioService);
  private readonly catalogo = inject(CatalogoService);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  mostrarProfileModal = false;
  profileName = '';
  profileEmail = '';
  currentSection = 'dashboard';
  cargando = false;

  readonly sidebarItems = [
    { section: 'dashboard', icon: '📊', label: 'Dashboard' },
    { section: 'team', icon: '👥', label: 'Mi Equipo' },
    { section: 'tickets', icon: '🎫', label: 'Supervisión Tickets' },
    { section: 'workload', icon: '⚖️', label: 'Carga de Trabajo' },
    { section: 'performance', icon: '📈', label: 'Rendimiento' },
    { section: 'escalations', icon: '🚨', label: 'Escalaciones' }
  ];

  dashboardStats = [
    { label: 'Tickets Activos', value: '0', trend: '', trendClass: 'text-blue-600', icon: '🎫', bgClass: 'bg-blue-100' },
    { label: 'Agentes Disponibles', value: '0', trend: '', trendClass: 'text-green-600', icon: '👨‍💻', bgClass: 'bg-green-100' },
    { label: 'Tiempo Promedio de Resolución', value: '-', trend: '', trendClass: 'text-green-600', icon: '⏱️', bgClass: 'bg-yellow-100' }
  ];

  priorityTickets: Ticket[] = [];
  teamMembers: TeamMember[] = [];
  tickets: Ticket[] = [];
  vencidos: Ticket[] = [];
  mostrarVencidos = false;
  tiempoResolucionPromedio = '-';
  escalations: Escalation[] = [];
  weeklyPerformance: RendimientoSemanal | null = null;
  agentComparison: ComparativaAgente[] = [];
  categorias: CategoriaTicket[] = [];

  selectedTicketIdPerAgent: Record<number, number | null> = {};

  // Filtros de supervisión de tickets
  filterEstado = '';
  filterPrioridad = '';
  filterAgente = '';
  filteredTickets: Ticket[] = [];

  // Supervisión de un ticket y su chat
  supervisandoTicket: Ticket | null = null;
  mensajes: MensajeSupervision[] = [];
  mensajeIntervencion = '';
  chatBloqueado = true;
  chatError = '';
  categoriaEscalada = '';
  agenteReasignado = '';

  // Reportes
  agenteReporte: number | null = null;
  generandoReporte = false;

  private graficas: Record<string, Chart | undefined> = {};

  get supervisorName(): string {
    return this.auth.usuario()?.nombreUsuario ?? 'Supervisor';
  }

  private get supervisorUserId(): number {
    return this.auth.usuario()?.idUsuario ?? 0;
  }

  ngOnInit(): void {
    this.catalogo.categorias$.subscribe(categorias => this.categorias = categorias);

    this.chatService.mensajes$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(mensaje => this.agregarMensajeEnVivo(mensaje));
  }

  ngAfterViewInit(): void {
    this.loadAllData();
  }

  ngOnDestroy(): void {
    void this.chatService.desconectar();
    Object.values(this.graficas).forEach(grafica => grafica?.destroy());
  }

  showSection(section: string): void {
    this.currentSection = section;

    if (section === 'workload' || section === 'performance') {
      this.loadAllData();
    } else {
      // Espera a que Angular pinte los <canvas> de la sección antes de dibujar.
      setTimeout(() => this.loadCharts());
    }
  }

  // ---------- Carga de datos ----------

  /** Pide todo en paralelo y dibuja cuando ha llegado todo (antes: cuatro banderas a mano). */
  loadAllData(): void {
    this.cargando = true;

    forkJoin({
      equipo: this.supervisorService.getTeamMembers(),
      tickets: this.supervisorService.getDashboardTickets(),
      prioritarios: this.supervisorService.getPriorityTickets(),
      escalaciones: this.supervisorService.getEscalations(),
      vencidos: this.supervisorService.getVencidos(),
      semana: this.supervisorService.getWeeklyPerformance(),
      comparativa: this.supervisorService.getAgentComparison()
    }).subscribe({
      next: datos => {
        this.teamMembers = datos.equipo;
        this.tickets = datos.tickets;
        this.priorityTickets = datos.prioritarios;
        this.escalations = datos.escalaciones;
        this.vencidos = datos.vencidos;
        this.weeklyPerformance = datos.semana;
        this.agentComparison = datos.comparativa;

        if (this.agenteReporte === null && this.teamMembers.length > 0) {
          this.agenteReporte = this.teamMembers[0].idAgente;
        }

        this.tiempoResolucionPromedio = this.getTiempoResolucionPromedio();
        this.dashboardStats[0].value = String(this.activeTickets.length);
        this.dashboardStats[1].value = String(this.getTeamStatus('available'));
        this.dashboardStats[2].value = this.tiempoResolucionPromedio;

        this.aplicarFiltros();
        this.cargando = false;
        setTimeout(() => this.loadCharts());
      },
      error: err => {
        this.cargando = false;
        alert(mensajeDeError(err, 'No se pudieron cargar los datos del panel.'));
      }
    });
  }

  // ---------- Indicadores ----------

  get activeTickets(): Ticket[] {
    return this.tickets.filter(t => esEstadoActivo(t.status));
  }

  /** Promedio diario de tickets resueltos en los últimos siete días (dato real del backend). */
  getTicketsPorDiaPromedio(): string {
    const resueltos = this.weeklyPerformance?.resolved ?? [];
    const total = resueltos.reduce((suma, valor) => suma + valor, 0);
    return (total / 7).toFixed(1);
  }

  /** Horas promedio entre apertura y última actualización de los tickets finalizados. */
  getTiempoResolucionPromedio(): string {
    const finalizados = this.tickets.filter(t =>
      esEstadoFinalizado(t.status) && t.fechaHoraCreacionTicket && t.fechaHoraActualizacionTicket);

    if (!finalizados.length) {
      return 'N/D';
    }

    const totalHoras = finalizados.reduce((suma, t) => {
      const inicio = new Date(t.fechaHoraCreacionTicket!).getTime();
      const fin = new Date(t.fechaHoraActualizacionTicket!).getTime();
      return suma + Math.max(0, (fin - inicio) / 3_600_000);
    }, 0);

    return (totalHoras / finalizados.length).toFixed(1) + 'h';
  }

  /** Porcentaje de tickets finalizados sobre asignados, sumando todo el equipo. */
  get tasaResolucionGlobal(): string {
    const asignados = this.agentComparison.reduce((suma, a) => suma + a.asignados, 0);
    const resueltos = this.agentComparison.reduce((suma, a) => suma + a.resueltos, 0);
    return asignados === 0 ? 'N/D' : `${((resueltos * 100) / asignados).toFixed(1)}%`;
  }

  getTeamStatus(status: string): number {
    return this.teamMembers.filter(m => m.status === status).length;
  }

  // ---------- Estilos ----------

  getPriorityBadgeClass(priority: string): string {
    const clases: Record<string, string> = {
      urgente: 'bg-red-100 text-red-800',
      alta: 'bg-orange-100 text-orange-800',
      media: 'bg-yellow-100 text-yellow-800',
      baja: 'bg-green-100 text-green-800'
    };
    return clases[(priority ?? '').toLowerCase()] ?? 'bg-gray-100 text-gray-800';
  }

  getStatusColor(status: string): string {
    const colores: Record<string, string> = {
      available: 'bg-green-500',
      busy: 'bg-yellow-500',
      away: 'bg-red-500'
    };
    return colores[status] ?? 'bg-gray-500';
  }

  getStatusBadgeClass(status: string): string {
    const clases: Record<string, string> = {
      'abierto': 'bg-blue-100 text-blue-800',
      'en-progreso': 'bg-yellow-100 text-yellow-800',
      'pendiente': 'bg-purple-100 text-purple-800',
      'resuelto': 'bg-green-100 text-green-800'
    };
    return clases[normalizarEstado(status)] ?? 'bg-gray-100 text-gray-800';
  }

  get escalacionesCriticas(): Escalation[] {
    return this.escalations.filter(e => e.status === 'critical');
  }

  get escalacionesPendientes(): Escalation[] {
    return this.escalations.filter(e => e.status === 'pending');
  }

  get escalacionesResueltas(): Escalation[] {
    return this.escalations.filter(e => e.status === 'resolved');
  }

  // ---------- Gráficas ----------

  loadCharts(): void {
    this.dibujar('ticketDistributionChart', canvas => new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels: this.teamMembers.map(m => m.name),
        datasets: [{
          data: this.teamMembers.map(m => m.tickets),
          backgroundColor: ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#ef4444', '#14b8a6']
        }]
      },
      options: { responsive: true, maintainAspectRatio: false }
    }));

    this.dibujar('workloadChart', canvas => new Chart(canvas, {
      type: 'bar',
      data: {
        labels: this.teamMembers.map(m => m.name),
        datasets: [{ label: 'Tickets activos', data: this.teamMembers.map(m => m.tickets), backgroundColor: '#3b82f6' }]
      },
      options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
    }));

    if (this.weeklyPerformance) {
      const semana = this.weeklyPerformance;
      this.dibujar('weeklyTrendChart', canvas => new Chart(canvas, {
        type: 'line',
        data: {
          labels: semana.labels,
          datasets: [
            { label: 'Resueltos', data: semana.resolved, borderColor: '#10b981', backgroundColor: 'rgba(16,185,129,0.1)', tension: 0.4 },
            { label: 'Creados', data: semana.created, borderColor: '#3b82f6', backgroundColor: 'rgba(59,130,246,0.1)', tension: 0.4 }
          ]
        },
        options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
      }));
    }

    // Comparativa real por agente: antes era un radar alimentado con números aleatorios.
    this.dibujar('agentComparisonChart', canvas => new Chart(canvas, {
      type: 'bar',
      data: {
        labels: this.agentComparison.map(a => a.name),
        datasets: [
          { label: 'Resueltos', data: this.agentComparison.map(a => a.resueltos), backgroundColor: '#10b981' },
          { label: 'Activos', data: this.agentComparison.map(a => a.activos), backgroundColor: '#f59e0b' }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
        plugins: {
          tooltip: {
            callbacks: {
              footer: elementos => {
                const agente = this.agentComparison[elementos[0]?.dataIndex ?? -1];
                if (!agente) {
                  return '';
                }
                const promedio = agente.tiempoPromedioHoras === null ? 'N/D' : `${agente.tiempoPromedioHoras} h`;
                return `Tasa de resolución: ${agente.tasaResolucion}% · Promedio: ${promedio}`;
              }
            }
          }
        }
      }
    }));
  }

  private dibujar(id: string, crear: (canvas: HTMLCanvasElement) => Chart): void {
    const canvas = document.getElementById(id) as HTMLCanvasElement | null;
    this.graficas[id]?.destroy();
    this.graficas[id] = canvas ? crear(canvas) : undefined;
  }

  // ---------- Filtros ----------

  aplicarFiltros(): void {
    this.filteredTickets = this.tickets.filter(ticket => {
      const estado = !this.filterEstado || normalizarEstado(ticket.status) === normalizarEstado(this.filterEstado);
      const prioridad = !this.filterPrioridad || (ticket.priority ?? '').toLowerCase() === this.filterPrioridad.toLowerCase();
      const agente = !this.filterAgente || ticket.agent === this.filterAgente;
      return estado && prioridad && agente;
    });
  }

  onEstadoChange(event: Event): void {
    this.filterEstado = (event.target as HTMLSelectElement).value;
    this.aplicarFiltros();
  }

  onPrioridadChange(event: Event): void {
    this.filterPrioridad = (event.target as HTMLSelectElement).value;
    this.aplicarFiltros();
  }

  onAgenteChange(event: Event): void {
    this.filterAgente = (event.target as HTMLSelectElement).value;
    this.aplicarFiltros();
  }

  // ---------- Acciones sobre tickets ----------

  assignTicket(ticketId: number | null, agenteId: number): void {
    if (!ticketId) {
      return;
    }

    this.supervisorService.asignarTicket(Number(ticketId), agenteId).subscribe({
      next: respuesta => {
        alert(respuesta.message);
        this.selectedTicketIdPerAgent[agenteId] = null;
        this.loadAllData();
      },
      error: err => alert(mensajeDeError(err, 'No se pudo asignar el ticket.'))
    });
  }

  redistribuirTickets(): void {
    this.supervisorService.redistribuir().subscribe({
      next: respuesta => {
        alert(respuesta.message);
        this.loadAllData();
      },
      error: err => alert(mensajeDeError(err, 'No se pudieron redistribuir los tickets.'))
    });
  }

  asignarSinAsignar(): void {
    this.supervisorService.asignarSinAgente().subscribe({
      next: respuesta => {
        alert(respuesta.message);
        this.loadAllData();
      },
      error: err => alert(mensajeDeError(err, 'No se pudieron asignar los tickets sin agente.'))
    });
  }

  /** Muestra la lista de tickets que superan el SLA (antes solo decía cuántos había). */
  revisarVencidos(): void {
    this.supervisorService.getVencidos().subscribe({
      next: vencidos => {
        this.vencidos = vencidos;
        this.mostrarVencidos = true;
      },
      error: err => alert(mensajeDeError(err, 'No se pudieron consultar los tickets vencidos.'))
    });
  }

  // ---------- Supervisión y chat ----------

  abrirSupervision(ticket: Ticket): void {
    this.supervisandoTicket = ticket;
    this.mensajes = [];
    this.mensajeIntervencion = '';
    this.categoriaEscalada = '';
    this.agenteReasignado = ticket.idAgenteAsignado ? String(ticket.idAgenteAsignado) : '';
    this.chatBloqueado = true;
    this.chatError = '';

    this.chatService.historial(ticket.id).subscribe({
      next: historial => {
        this.mensajes = historial.map(m => ({
          remitente: m.usuarioNombre,
          texto: m.mensajeTicket,
          esSupervisor: m.idUsuario === this.supervisorUserId
        }));
      },
      error: err => this.chatError = mensajeDeError(err, 'No se pudo cargar el historial.')
    });

    this.chatService.conectar(ticket.id)
      .catch(() => this.chatError = 'No se pudo conectar al chat en tiempo real.');
  }

  intervenirChat(): void {
    this.chatBloqueado = false;
  }

  enviarMensaje(): void {
    const texto = this.mensajeIntervencion.trim();
    if (!texto || !this.supervisandoTicket) {
      return;
    }

    this.mensajeIntervencion = '';
    this.chatService.enviar(texto).catch(err => {
      this.mensajeIntervencion = texto;
      this.chatError = err?.message ?? 'No se pudo enviar el mensaje.';
    });
  }

  cerrarSupervision(): void {
    this.supervisandoTicket = null;
    this.mensajes = [];
    void this.chatService.desconectar();
  }

  reasignarAgente(): void {
    if (!this.supervisandoTicket || !this.agenteReasignado) {
      return;
    }

    const idAgente = Number(this.agenteReasignado);

    this.supervisorService.asignarTicket(this.supervisandoTicket.id, idAgente).subscribe({
      next: respuesta => {
        alert(respuesta.message);
        if (this.supervisandoTicket) {
          this.supervisandoTicket.agent = this.teamMembers.find(a => a.idAgente === idAgente)?.name ?? '';
          this.supervisandoTicket.idAgenteAsignado = idAgente;
        }
        this.loadAllData();
      },
      error: err => alert(mensajeDeError(err, 'No se pudo reasignar el ticket.'))
    });
  }

  escalarTicket(): void {
    if (!this.supervisandoTicket || !this.categoriaEscalada) {
      return;
    }

    this.supervisorService.escalarTicket(this.supervisandoTicket.id, this.categoriaEscalada).subscribe({
      next: respuesta => {
        alert(respuesta.message);
        if (this.supervisandoTicket) {
          this.supervisandoTicket.category = this.categoriaEscalada;
        }
        this.loadAllData();
      },
      error: err => alert(mensajeDeError(err, 'No se pudo escalar el ticket.'))
    });
  }

  private agregarMensajeEnVivo(mensaje: MensajeChatEnVivo): void {
    if (mensaje.idTicket !== this.supervisandoTicket?.id) {
      return;
    }

    this.mensajes.push({
      remitente: mensaje.usuario,
      texto: mensaje.mensaje,
      esSupervisor: mensaje.idUsuario === this.supervisorUserId
    });
  }

  // ---------- Reportes ----------

  generarReporteCarga(): void {
    this.descargar(this.supervisorService.reporteCarga(), 'reporte-carga.pdf');
  }

  generarReporteSemanal(): void {
    this.descargar(this.supervisorService.reporteSemanal(), 'reporte-semanal.pdf');
  }

  generarReporteIndividual(): void {
    if (this.agenteReporte === null) {
      return;
    }
    this.descargar(
      this.supervisorService.reporteIndividual(this.agenteReporte),
      `reporte-agente-${this.agenteReporte}.pdf`);
  }

  private descargar(peticion: ReturnType<SupervisorService['reporteCarga']>, nombre: string): void {
    this.generandoReporte = true;

    peticion.subscribe({
      next: blob => {
        descargarArchivo(blob, nombre);
        this.generandoReporte = false;
      },
      error: async err => {
        this.generandoReporte = false;
        alert(await mensajeDeErrorBlob(err, 'No se pudo generar el reporte.'));
      }
    });
  }

  // ---------- Perfil y sesión ----------

  showProfileModal(): void {
    this.profileName = this.supervisorName;
    this.profileEmail = this.auth.usuario()?.correoUsuario ?? '';
    this.mostrarProfileModal = true;
  }

  closeProfileModal(): void {
    this.mostrarProfileModal = false;
  }

  /** Antes solo se guardaba en localStorage; ahora se persiste en el servidor. */
  updateProfile(): void {
    const nombre = this.profileName.trim() || this.supervisorName;

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
