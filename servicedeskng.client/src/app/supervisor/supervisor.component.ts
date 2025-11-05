import { Component, AfterViewInit, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SupervisorService, TeamMember, Ticket, Escalation } from './supervisor.service';
import { ChatService } from '../chat/chat.service';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-supervisor',
  templateUrl: './supervisor.component.html',
  styleUrls: ['./supervisor.component.css']
})
export class SupervisorComponent implements AfterViewInit, OnInit, OnDestroy {
  supervisorName = '';
  supervisorId = 0;
  // Profile modal state and fields (used by template)
  mostrarProfileModal: boolean = false;
  profileName: string = '';
  profileEmail: string = '';
  currentSection = 'dashboard';

  sidebarItems = [
    { section: 'dashboard', icon: '📊', label: 'Dashboard' },
    { section: 'team', icon: '👥', label: 'Mi Equipo' },
    { section: 'tickets', icon: '🎫', label: 'Supervisión Tickets' },
    { section: 'workload', icon: '⚖️', label: 'Carga de Trabajo' },
    { section: 'performance', icon: '📈', label: 'Rendimiento' },
    { section: 'escalations', icon: '🚨', label: 'Escalaciones' }
    // No incluir sección de reportes
  ];

  dashboardStats = [
    { label: 'Tickets Activos', value: 0, trend: '', trendClass: 'text-blue-600', icon: '🎫', bgClass: 'bg-blue-100' },
    { label: 'Agentes Activos', value: 0, trend: '', trendClass: 'text-green-600', icon: '👨‍💻', bgClass: 'bg-green-100' },
    { label: 'Tiempo Promedio', value: '', trend: '', trendClass: 'text-green-600', icon: '⏱️', bgClass: 'bg-yellow-100' }
  ];

  priorityTickets: Ticket[] = [];
  teamMembers: TeamMember[] = [];
  tickets: Ticket[] = [];
  tiempoResolucionPromedio: string = '';
  escalations: Escalation[] = [];
  weeklyPerformance: any = null;
  agentComparison: any = null;
  private workloadChartInstance: any;
  private ticketDistributionChartInstance: any;
  private weeklyTrendChartInstance: any;
  private agentComparisonChartInstance: any;
  selectedTicketId: number | null = null;
  selectedTicketIdPerAgent: { [idAgente: number]: string | null } = {};

  // Filtros para supervisión de tickets
  filterEstado: string = '';
  filterPrioridad: string = '';
  filterAgente: string = '';
  filteredTickets: Ticket[] = [];

  // Nueva función para aplicar todos los filtros
  aplicarFiltros() {
    let baseTickets = this.tickets;
    // Si hay filtro de agente, usa los tickets filtrados por agente
    if (this.filterAgente) {
      const agente = this.teamMembers.find(a => a.name === this.filterAgente);
      if (agente) {
        this.supervisorService.getTicketsByAgente(agente.idAgente).subscribe(tickets => {
          this.filteredTickets = tickets.filter(ticket => {
            const estadoMatch = !this.filterEstado || ticket.status?.toLowerCase() === this.filterEstado.toLowerCase();
            const prioridadMatch = !this.filterPrioridad || ticket.priority?.toLowerCase() === this.filterPrioridad.toLowerCase();
            return estadoMatch && prioridadMatch;
          });
        });
        return; // Espera la respuesta del backend
      }
    }
    // Si no hay filtro de agente, filtra sobre todos los tickets
    this.filteredTickets = baseTickets.filter(ticket => {
      const estadoMatch = !this.filterEstado || ticket.status?.toLowerCase() === this.filterEstado.toLowerCase();
      const prioridadMatch = !this.filterPrioridad || ticket.priority?.toLowerCase() === this.filterPrioridad.toLowerCase();
      return estadoMatch && prioridadMatch;
    });
  }

  // Supervisión de ticket y chat
  supervisandoTicket: Ticket | null = null;
  mensajes: any[] = [];
  mensajeIntervencion: string = '';
  chatBloqueado: boolean = true;
  categoriaEscalada: string = '';
  agenteReasignado: string = '';

  private chatSub: any;

  constructor(
    private supervisorService: SupervisorService,
    private http: HttpClient,
    private chatService: ChatService
  ) {}

  ngOnInit() {
    this.supervisorName = localStorage.getItem('usuario') || 'Supervisor';
    this.supervisorId = +(localStorage.getItem('usuarioId') || 0);
    // Inicializa el agente individual con el primero disponible
    this.supervisorService.getTeamMembers().subscribe(data => {
      this.teamMembers = data;
      if (data.length > 0) {
        // Eliminar: this.reporteIndividualAgente = data[0].name;
      }
    });
    // Initialize profile fields
    this.profileName = this.supervisorName;
    this.profileEmail = localStorage.getItem('usuarioEmail') || '';
    // Inicializa los tickets filtrados
    this.filteredTickets = this.tickets;
  }

  ngAfterViewInit() {
    this.loadAllData();
  }

  ngOnDestroy() {
    if (this.supervisandoTicket) {
      this.chatService.disconnect(this.supervisandoTicket.id.toString());
    }
    if (this.chatSub) {
      this.chatSub.unsubscribe();
    }
  }

  showSection(section: string) {
    this.currentSection = section;
    if (section === 'workload' || section === 'performance') {
      this.loadAllData();
      // Espera a que Angular renderice el HTML antes de cargar las gráficas
      setTimeout(() => this.loadCharts(), 500);
    } else {
      setTimeout(() => this.loadCharts(), 0);
    }
  }

  showNotifications() {
    alert('Panel de notificaciones en desarrollo');
  }

  logout() {
    if (confirm('¿Estás seguro de que quieres cerrar sesión?')) {
      localStorage.clear();
      window.location.href = '/'; // Redirige al home/login
    }
  }

  // Template calls confirmLogout(); provide wrapper to keep naming in template
  confirmLogout() {
    this.logout();
  }

  loadAllData() {
    let teamLoaded = false, ticketsLoaded = false, weeklyLoaded = false, agentCompLoaded = false;
    const checkAndLoadCharts = () => {
      if (teamLoaded && ticketsLoaded && weeklyLoaded && agentCompLoaded) {
        console.log('Datos para rendimiento:', {
          weeklyPerformance: this.weeklyPerformance,
          agentComparison: this.agentComparison
        });
        this.loadCharts();
      }
    };
    this.supervisorService.getTeamMembers().subscribe(data => {
      this.teamMembers = data;
      this.dashboardStats[1].value = data.length;
      this.dashboardStats[2].value = this.getAverageTime();
      teamLoaded = true;
      checkAndLoadCharts();
    });
    this.supervisorService.getDashboardTickets().subscribe(data => {
      this.tickets = data;
      this.filteredTickets = data;
      this.dashboardStats[0].value = data.filter(t => t.status === 'Abierto' || t.status === 'en-progreso').length;
      this.tiempoResolucionPromedio = this.getTiempoResolucionPromedio();
      ticketsLoaded = true;
      checkAndLoadCharts();
    });
    this.supervisorService.getEscalations().subscribe(data => {
      this.escalations = data;
    });
    this.supervisorService.getWeeklyPerformance().subscribe(data => {
      this.weeklyPerformance = data;
      weeklyLoaded = true;
      checkAndLoadCharts();
    });
    this.supervisorService.getAgentComparison().subscribe(data => {
      this.agentComparison = data;
      agentCompLoaded = true;
      checkAndLoadCharts();
    });
  }

  getAverageTime(): string {
    if (!this.teamMembers.length) return '';
    const times = this.teamMembers.map(m => parseFloat(m.avgTime));
    const avg = times.reduce((a, b) => a + b, 0) / times.length;
    return avg.toFixed(1) + 'h';
  }

  getTicketsPorDiaPromedio() {
    // Simulación: tickets cerrados en los últimos 7 días / 7
    const ahora = new Date();
    const hace7 = new Date(ahora.getTime() - 7 * 24 * 60 * 60 * 1000);
    const cerrados = this.tickets.filter(t => (t.status?.toLowerCase() === 'resuelto' || t.status?.toLowerCase() === 'cerrado'));
    // Si tu backend tiene fecha de cierre, úsala aquí
    return (cerrados.length / 7).toFixed(1);
  }

  // Calcula el tiempo promedio real de resolución de tickets cerrados/resueltos
  getTiempoResolucionPromedio() {
    // Filtra tickets resueltos/cerrados
    const cerrados = this.tickets.filter(t =>
      (t.status?.toLowerCase() === 'resuelto' || t.status?.toLowerCase() === 'cerrado') &&
      t.fechaHoraCreacionTicket && t.fechaHoraActualizacionTicket
    );
    if (!cerrados.length) return '0h';
    let totalHoras = 0;
    cerrados.forEach(t => {
      const inicio = new Date(t.fechaHoraCreacionTicket as string).getTime();
      const fin = new Date(t.fechaHoraActualizacionTicket as string).getTime();
      const diffHoras = (fin - inicio) / (1000 * 60 * 60);
      totalHoras += diffHoras > 0 ? diffHoras : 0;
    });
    const promedio = totalHoras / cerrados.length;
    return promedio.toFixed(1) + 'h';
  }

  getTeamStatus(status: string): number {
    return this.teamMembers.filter(m => m.status === status).length;
  }

  getPriorityIcon(priority: string) {
    const icons: any = { urgent: '🚨', high: '⚠️', medium: '📋', low: '📝' };
    return icons[priority] || '📋';
  }

  getPriorityClass(priority: string) {
    return {
      'priority-urgent': priority === 'urgent',
      'priority-high': priority === 'high',
      'priority-medium': priority === 'medium',
      'priority-low': priority === 'low'
    };
  }

  getPriorityBadgeClass(priority: string) {
    const classes: any = {
      urgent: 'bg-red-100 text-red-800',
      high: 'bg-orange-100 text-orange-800',
      medium: 'bg-yellow-100 text-yellow-800',
      low: 'bg-green-100 text-green-800'
    };
    return classes[priority] || 'bg-gray-100 text-gray-800';
  }

  getStatusColor(status: string) {
    const colors: any = {
      available: 'bg-green-500',
      busy: 'bg-yellow-500',
      away: 'bg-red-500'
    };
    return colors[status] || 'bg-gray-500';
  }

  getCategoryIcon(category: string): string {
    const icons: any = {
      'Infraestructura': '🏗️',
      'Base de Datos': '🗄️',
      'Software': '💾',
      'Hardware': '🖥️',
      'Red': '🌐'
    };
    return icons[category] || '❓';
  }

  getStatusBadgeClass(status: string): string {
    const classes: any = {
      'abierto': 'bg-blue-100 text-blue-800',
      'en-progreso': 'bg-yellow-100 text-yellow-800',
      'pendiente': 'bg-purple-100 text-purple-800',
      'resuelto': 'bg-green-100 text-green-800'
    };
    return classes[status] || 'bg-gray-100 text-gray-800';
  }

  getEscalationCount(type: string): number {
    return this.escalations.filter(e => e.status === type).length;
  }

  loadCharts() {
    const ticketDistributionElem = document.getElementById('ticketDistributionChart');
    if (ticketDistributionElem) {
      if (this.ticketDistributionChartInstance) {
        this.ticketDistributionChartInstance.destroy();
      }
      this.ticketDistributionChartInstance = new Chart(ticketDistributionElem as HTMLCanvasElement, {
        type: 'doughnut',
        data: {
          labels: this.teamMembers.map(m => m.name),
          datasets: [{
            data: this.teamMembers.map(m => m.tickets),
            backgroundColor: [
              'rgba(59, 130, 246, 0.8)',
              'rgba(16, 185, 129, 0.8)',
              'rgba(245, 158, 11, 0.8)',
              'rgba(139, 92, 246, 0.8)'
            ]
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }
    const workloadChartElem = document.getElementById('workloadChart');
    if (workloadChartElem) {
      if (this.workloadChartInstance) {
        this.workloadChartInstance.destroy();
      }
      this.workloadChartInstance = new Chart(workloadChartElem as HTMLCanvasElement, {
        type: 'bar',
        data: {
          labels: this.teamMembers.map(m => m.name),
          datasets: [{
            label: 'Tickets Asignados',
            data: this.teamMembers.map(m => m.tickets),
            backgroundColor: 'rgba(59, 130, 246, 0.8)'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { y: { beginAtZero: true } }
        }
      });
    }
    const weeklyTrendElem = document.getElementById('weeklyTrendChart');
    console.log('weeklyTrendElem:', weeklyTrendElem, 'weeklyPerformance:', this.weeklyPerformance);
    if (weeklyTrendElem && this.weeklyPerformance) {
      if (this.weeklyTrendChartInstance) {
        this.weeklyTrendChartInstance.destroy();
      }
      this.weeklyTrendChartInstance = new Chart(weeklyTrendElem as HTMLCanvasElement, {
        type: 'line',
        data: {
          labels: this.weeklyPerformance.labels,
          datasets: [
            {
              label: 'Tickets Resueltos',
              data: this.weeklyPerformance.resolved,
              borderColor: 'rgba(16, 185, 129, 1)',
              backgroundColor: 'rgba(16, 185, 129, 0.1)',
              tension: 0.4
            },
            {
              label: 'Tickets Creados',
              data: this.weeklyPerformance.created,
              borderColor: 'rgba(59, 130, 246, 1)',
              backgroundColor: 'rgba(59, 130, 246, 0.1)',
              tension: 0.4
            }
          ]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }
    const agentComparisonElem = document.getElementById('agentComparisonChart');
    console.log('agentComparisonElem:', agentComparisonElem, 'agentComparison:', this.agentComparison);
    if (agentComparisonElem && this.agentComparison) {
      if (this.agentComparisonChartInstance) {
        this.agentComparisonChartInstance.destroy();
      }
      this.agentComparisonChartInstance = new Chart(agentComparisonElem as HTMLCanvasElement, {
        type: 'radar',
        data: {
          labels: ['Velocidad', 'Calidad', 'Satisfacción', 'Comunicación', 'Proactividad'],
          datasets: this.agentComparison.map((a: any) => ({
            label: a.name,
            data: [a.velocidad, a.calidad, a.satisfaccion, a.comunicacion, a.proactividad],
            borderColor: 'rgba(59, 130, 246, 1)',
            backgroundColor: 'rgba(59, 130, 246, 0.2)'
          }))
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { r: { beginAtZero: true, max: 10 } }
        }
      });
    }
  }

  assignTicket(ticketId: string | null, agenteId: number) {
    if (!ticketId) return;
    this.http.post('/api/tickets/assign', { idTicket: Number(ticketId), idAgente: agenteId }).subscribe({
      next: () => {
        alert('Ticket asignado correctamente');
        this.loadAllData();
        this.selectedTicketIdPerAgent[agenteId] = null;
      },
      error: () => {
        alert('Error al asignar el ticket');
      }
    });
  }

  // Cuando el usuario seleccione un ticket para asignar, actualiza selectedTicketId
  selectTicket(ticketId: number) {
    this.selectedTicketId = ticketId;
  }

  get activeTickets() {
    // Ajusta los estados según tu lógica de "activo"
    return this.tickets.filter(t => t.status === 'Abierto' || t.status === 'en-progreso');
  }

  onEstadoChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.filterEstado = select.value;
    this.aplicarFiltros();
  }

  onPrioridadChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.filterPrioridad = select.value;
    this.aplicarFiltros();
  }

  onAgenteChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.filterAgente = select.value;
    this.aplicarFiltros();
  }

  abrirSupervision(ticket: Ticket) {
    if (this.supervisandoTicket) {
      this.chatService.disconnect(this.supervisandoTicket.id.toString());
    }
    this.supervisandoTicket = ticket;
    this.mensajes = [];
    this.mensajeIntervencion = '';
    this.categoriaEscalada = '';
    this.agenteReasignado = ticket.agent;
    this.chatBloqueado = true;
    // Cargar mensajes históricos
    this.chatService.getMensajesPorTicket(Number(ticket.id)).subscribe(mensajes => {
      this.mensajes = mensajes.map(m => ({
        remitente: m.usuarioNombre || m.nombreUsuario || m.usuario, // Siempre usar el nombre guardado
        texto: m.mensajeTicket,
        esSupervisor: m.idUsuario === this.supervisorId
      }));
    });
    // Conecta al hub y suscríbete a los mensajes en tiempo real
    this.chatService.connect(ticket.id.toString());
    this.chatService.onReceiveMessage((user, message, fecha) => {
      this.mensajes.push({
        remitente: user, // Usar siempre el nombre recibido
        texto: message,
        esSupervisor: user === this.supervisorName
      });
    });
  }

  intervenirChat() {
    this.chatBloqueado = false;
  }

  enviarMensaje() {
    if (!this.mensajeIntervencion.trim() || !this.supervisandoTicket) return;
    this.chatService.sendMessage(
      this.supervisandoTicket.id.toString(),
      this.supervisorName,
      this.mensajeIntervencion,
      this.supervisorId
    );
    this.mensajeIntervencion = '';
  }

  cerrarSupervision() {
    if (this.supervisandoTicket) {
      this.chatService.disconnect(this.supervisandoTicket.id.toString());
    }
    this.supervisandoTicket = null;
    this.mensajes = [];
  }

  reasignarAgente() {
    if (this.supervisandoTicket) {
      this.http.post('/api/tickets/assign', {
        idTicket: this.supervisandoTicket.id,
        idAgente: this.agenteReasignado // ahora es el id numérico
      }).subscribe({
        next: () => {
          alert('Ticket reasignado correctamente');
          if (this.supervisandoTicket) {
            // Actualiza el nombre del agente en el ticket local
            const agente = this.teamMembers.find(a => a.idAgente === Number(this.agenteReasignado));
            this.supervisandoTicket.agent = agente ? agente.name : '';
          }
          this.loadAllData();
        },
        error: () => {
          alert('Error al reasignar el ticket');
        }
      });
    }
  }

  escalarTicket() {
    if (this.supervisandoTicket) {
      this.http.post(`/api/tickets/${this.supervisandoTicket.id}/escalar`, {
        nuevaCategoria: this.categoriaEscalada
      }).subscribe({
        next: () => {
          alert('Ticket escalado correctamente');
          if (this.supervisandoTicket) {
            this.supervisandoTicket.category = this.categoriaEscalada;
          }
          this.loadAllData();
        },
        error: () => {
          alert('Error al escalar el ticket');
        }
      });
    }
  }

  redistribuirTickets() {
    this.http.post('/api/tickets/redistribuir', {}).subscribe({
      next: () => {
        alert('Tickets redistribuidos automáticamente');
        this.loadAllData();
      },
      error: () => {
        alert('Error al redistribuir tickets');
      }
    });
  }

  asignarSinAsignar() {
    this.http.post('/api/tickets/asignar-sin-agente', {}).subscribe({
      next: () => {
        alert('Tickets sin asignar han sido distribuidos');
        this.loadAllData();
      },
      error: () => {
        alert('Error al asignar tickets sin agente');
      }
    });
  }

  revisarVencidos() {
    this.http.get('/api/tickets/vencidos').subscribe({
      next: (tickets: any) => {
        alert(`Tickets vencidos encontrados: ${tickets.length}`);
        // Aquí podrías mostrar los tickets en un modal o sección
      },
      error: () => {
        alert('Error al revisar tickets vencidos');
      }
    });
  }

  get escalacionesActivas() {
    return this.escalations.filter(e => e.status === 'critical' || e.status === 'pending');
  }
  get escalacionesCriticas() {
    return this.escalations.filter(e => e.status === 'critical');
  }
  get escalacionesPendientes() {
    return this.escalations.filter(e => e.status === 'pending');
  }
  get escalacionesResueltas() {
    return this.escalations.filter(e => e.status === 'resolved');
  }
  get escalacionesEscaladas() {
    return this.escalations.filter(e => e.status === 'escalated');
  }

  // Show profile modal (called from template gears)
  showProfileModal() {
    this.profileName = this.supervisorName;
    this.profileEmail = localStorage.getItem('usuarioEmail') || '';
    this.mostrarProfileModal = true;
  }

  closeProfileModal() {
    this.mostrarProfileModal = false;
  }

  updateProfile() {
    // Persist profile changes locally (adapt to API if available)
    this.supervisorName = this.profileName;
    if (this.profileEmail) {
      localStorage.setItem('usuarioEmail', this.profileEmail);
    }
    localStorage.setItem('usuario', this.supervisorName);
    this.closeProfileModal();
  }

  generarReporteCarga() {
    this.http.get('/api/tickets/reporte-carga', { responseType: 'blob' }).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'reporte-carga.pdf';
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}
