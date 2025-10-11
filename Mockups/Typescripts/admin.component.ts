import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import Chart from 'chart.js/auto';
import { AdminService } from './services/admin.service';
import { User } from './models/user.model';
import { Ticket } from './models/ticket.model';
import { AuditLog } from './models/audit-log.model';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit, AfterViewInit {
  // Datos principales
  users: User[] = [];
  tickets: Ticket[] = [];
  auditLogs: AuditLog[] = [];

  // Filtros y estado UI
  filteredUsers: User[] = [];
  filteredTickets: Ticket[] = [];
  filteredLogs: AuditLog[] = [];
  currentSection = 'dashboard';
  notification: { message: string; type: string } | null = null;

  // Referencias a charts
  @ViewChild('statusChart') statusChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('trendChart') trendChartRef!: ElementRef<HTMLCanvasElement>;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngAfterViewInit(): void {
    this.renderCharts();
  }

  // ---------------- DATA ----------------
  loadData(): void {
    this.adminService.getUsers().subscribe(data => {
      this.users = data;
      this.filteredUsers = data;
    });
    this.adminService.getTickets().subscribe(data => {
      this.tickets = data;
      this.filteredTickets = data;
    });
    this.adminService.getAuditLogs().subscribe(data => {
      this.auditLogs = data;
      this.filteredLogs = data;
    });
  }

  // ---------------- UI HANDLERS ----------------
  showSection(section: string): void {
    this.currentSection = section;
  }

  filterUsers(search: string, role: string): void {
    this.filteredUsers = this.users.filter(u =>
      u.nombre.toLowerCase().includes(search.toLowerCase()) &&
      (role ? u.rol.toLowerCase() === role.toLowerCase() : true)
    );
  }

  filterTickets(search: string, status: string, category: string): void {
    this.filteredTickets = this.tickets.filter(t =>
      (t.titulo.toLowerCase().includes(search.toLowerCase()) ||
       t.usuario.toLowerCase().includes(search.toLowerCase())) &&
      (status ? t.estado.toLowerCase() === status : true) &&
      (category ? t.categoria.toLowerCase() === category : true)
    );
  }

  filterLogs(search: string): void {
    this.filteredLogs = this.auditLogs.filter(l =>
      l.usuario.toLowerCase().includes(search.toLowerCase()) ||
      l.accion.toLowerCase().includes(search.toLowerCase()) ||
      l.detalle.toLowerCase().includes(search.toLowerCase())
    );
  }

  showNotification(message: string, type: 'success' | 'error' | 'info' | 'warning' = 'info'): void {
    this.notification = { message, type };
    setTimeout(() => this.notification = null, 4000);
  }

  // ---------------- CHARTS ----------------
  renderCharts(): void {
    const statusCtx = this.statusChartRef?.nativeElement?.getContext('2d');
    if (statusCtx) {
      new Chart(statusCtx, {
        type: 'doughnut',
        data: {
          labels: ['Abiertos', 'En Progreso', 'Pendientes', 'Resueltos'],
          datasets: [{
            data: [45, 32, 18, 156],
            backgroundColor: ['#3b82f6', '#f59e0b', '#8b5cf6', '#10b981']
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }
  }

  // ---------------- UTIL ----------------
  getRoleBadgeClass(rol: string): string {
    const map: Record<string, string> = {
      Cliente: 'bg-blue-100 text-blue-800',
      Agente: 'bg-green-100 text-green-800',
      Supervisor: 'bg-yellow-100 text-yellow-800',
      Administrador: 'bg-purple-100 text-purple-800'
    };
    return map[rol] || 'bg-gray-100 text-gray-800';
  }
}
