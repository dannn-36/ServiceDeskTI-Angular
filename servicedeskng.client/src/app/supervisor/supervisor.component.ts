import { Component, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SupervisorService, TeamMember, Ticket, Escalation } from './supervisor.service';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-supervisor',
  templateUrl: './supervisor.component.html',
  styleUrls: ['./supervisor.component.css']
})
export class SupervisorComponent implements AfterViewInit {
  supervisorName = 'Roberto Silva';
  currentSection = 'dashboard';

  sidebarItems = [
    { section: 'dashboard', icon: '📊', label: 'Dashboard' },
    { section: 'team', icon: '👥', label: 'Mi Equipo' },
    { section: 'tickets', icon: '🎫', label: 'Supervisión Tickets' },
    { section: 'workload', icon: '⚖️', label: 'Carga de Trabajo' },
    { section: 'performance', icon: '📈', label: 'Rendimiento' },
    { section: 'escalations', icon: '🚨', label: 'Escalaciones' },
    { section: 'reports', icon: '📋', label: 'Reportes' }
  ];

  dashboardStats = [
    { label: 'Tickets Activos', value: 0, trend: '', trendClass: 'text-blue-600', icon: '🎫', bgClass: 'bg-blue-100' },
    { label: 'Agentes Activos', value: 0, trend: '', trendClass: 'text-green-600', icon: '👨‍💻', bgClass: 'bg-green-100' },
    { label: 'Tiempo Promedio', value: '', trend: '', trendClass: 'text-green-600', icon: '⏱️', bgClass: 'bg-yellow-100' },
    { label: 'SLA Cumplimiento', value: '', trend: '', trendClass: 'text-green-600', icon: '🎯', bgClass: 'bg-purple-100' }
  ];

  priorityTickets: Ticket[] = [];
  teamMembers: TeamMember[] = [];
  tickets: Ticket[] = [];
  escalations: Escalation[] = [];
  private workloadChartInstance: any;
  private ticketDistributionChartInstance: any;
  private weeklyTrendChartInstance: any;
  private agentComparisonChartInstance: any;
  selectedTicketId: number | null = null;

  constructor(private supervisorService: SupervisorService, private http: HttpClient) {}

  ngAfterViewInit() {
    this.loadAllData();
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
      alert('Cerrando sesión...');
    }
  }

  loadAllData() {
    let teamLoaded = false, priorityLoaded = false, ticketsLoaded = false;
    const checkAndLoadCharts = () => {
      if (teamLoaded && priorityLoaded && ticketsLoaded) {
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
    this.supervisorService.getPriorityTickets().subscribe(data => {
      this.priorityTickets = data;
      this.dashboardStats[0].value = data.length;
      priorityLoaded = true;
      checkAndLoadCharts();
    });
    this.supervisorService.getDashboardTickets().subscribe(data => {
      this.tickets = data;
      // Filtra los tickets activos según el campo 'status'
      this.dashboardStats[0].value = data.filter(t => t.status === 'Abierto' || t.status === 'en-progreso').length;
      ticketsLoaded = true;
      checkAndLoadCharts();
    });
    this.supervisorService.getEscalations().subscribe(data => {
      this.escalations = data;
    });
  }

  getAverageTime(): string {
    if (!this.teamMembers.length) return '';
    const times = this.teamMembers.map(m => parseFloat(m.avgTime));
    const avg = times.reduce((a, b) => a + b, 0) / times.length;
    return avg.toFixed(1) + 'h';
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
    if (weeklyTrendElem) {
      if (this.weeklyTrendChartInstance) {
        this.weeklyTrendChartInstance.destroy();
      }
      this.weeklyTrendChartInstance = new Chart(weeklyTrendElem as HTMLCanvasElement, {
        type: 'line',
        data: {
          labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
          datasets: [{
            label: 'Tickets Resueltos',
            data: [12, 15, 18, 14, 16, 8, 5],
            borderColor: 'rgba(16, 185, 129, 1)',
            backgroundColor: 'rgba(16, 185, 129, 0.1)',
            tension: 0.4
          }, {
            label: 'Tickets Creados',
            data: [10, 16, 20, 15, 18, 10, 7],
            borderColor: 'rgba(59, 130, 246, 1)',
            backgroundColor: 'rgba(59, 130, 246, 0.1)',
            tension: 0.4
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }
    const agentComparisonElem = document.getElementById('agentComparisonChart');
    if (agentComparisonElem) {
      if (this.agentComparisonChartInstance) {
        this.agentComparisonChartInstance.destroy();
      }
      this.agentComparisonChartInstance = new Chart(agentComparisonElem as HTMLCanvasElement, {
        type: 'radar',
        data: {
          labels: ['Velocidad', 'Calidad', 'Satisfacción', 'Comunicación', 'Proactividad'],
          datasets: [
            {
              label: this.teamMembers[0]?.name || '',
              data: [9, 8, 9, 8, 7],
              borderColor: 'rgba(59, 130, 246, 1)',
              backgroundColor: 'rgba(59, 130, 246, 0.2)'
            },
            {
              label: this.teamMembers[1]?.name || '',
              data: [7, 9, 8, 9, 8],
              borderColor: 'rgba(16, 185, 129, 1)',
              backgroundColor: 'rgba(16, 185, 129, 0.2)'
            }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { r: { beginAtZero: true, max: 10 } }
        }
      });
    }
  }

  assignTicket(ticketId: number, agenteId: number) {
    this.http.post('/api/tickets/assign', { idTicket: ticketId, idAgente: agenteId }).subscribe({
      next: () => {
        alert('Ticket asignado correctamente');
        this.loadAllData(); // Recargar datos
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
}
