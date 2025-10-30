import { Component, OnInit, AfterViewInit } from '@angular/core';
import { UsuarioService, Usuario } from '../usuario/usuario.service';
import { TicketsService, Ticket } from '../tickets/tickets.service';
import { HttpClient } from '@angular/common/http';
import Chart from 'chart.js/auto';

interface EstadoTicket {
  idEstado: number;
  nombreEstado: string;
}

@Component({
  selector: 'app-administrador',
  templateUrl: './administrador.component.html',
  styleUrls: ['./administrador.component.css']
})
export class AdministradorComponent implements OnInit, AfterViewInit {
  // ...usuarios...
  users: Usuario[] = [];
  filteredUsers: Usuario[] = [];
  userSearch: string = '';
  userRoleFilter: string = '';
  showUserModal = false;
  loadingUsers = false;
  userError = '';
  selectedUser: Usuario | null = null;
  formUser: Partial<Usuario> = { tipoUsuario: 'Cliente' };
  userEditMode = false;

  // Secciones y perfil
  currentSection: string = 'dashboard';
  showProfile = false;
  profileName = 'Admin Sistema';
  profileEmail = 'admin@empresa.com';

  // Contadores de roles
  totalClientes = 0;
  totalAgentes = 0;
  totalSupervisores = 0;
  totalAdministradores = 0;

  // --- TICKETS ---
  tickets: Ticket[] = [];
  filteredTickets: Ticket[] = [];
  ticketSearch: string = '';
  ticketStatusFilter: string = '';
  ticketCategoryFilter: string = '';
  loadingTickets = false;
  ticketError = '';
  estados: EstadoTicket[] = [];
  categorias: any[] = [];

  // Contadores por estado
  totalAbiertos = 0;
  totalEnProgreso = 0;
  totalPendientes = 0;
  totalResueltos = 0;
  totalUrgentes = 0;

  // Gráficas
  statusChart: any;
  trendChart: any;

  constructor(
    private usuarioService: UsuarioService,
    private ticketsService: TicketsService,
    private http: HttpClient
  ) { }

  ngOnInit(): void {
    this.profileName = localStorage.getItem('usuario') || 'Admin Sistema';
    this.profileEmail = localStorage.getItem('usuarioEmail') || 'admin@empresa.com';
    this.getUsers();
    this.filterUsers();
    this.cargarEstados();
    this.cargarCategorias();
    this.getTickets();
  }

  ngAfterViewInit(): void { }

  getMonthlyTicketStats() {
    // Agrupa tickets por mes de creación
    const months = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
    const createdCounts = Array(12).fill(0);
    const resolvedCounts = Array(12).fill(0);
    this.tickets.forEach(ticket => {
      if (ticket.fechaHoraCreacionTicket) {
        const date = new Date(ticket.fechaHoraCreacionTicket);
        const month = date.getMonth();
        createdCounts[month]++;
        if (this.getEstadoNombre(ticket.idEstadoTicket) === 'Resuelto') {
          resolvedCounts[month]++;
        }
      }
    });
    return {
      labels: months,
      created: createdCounts,
      resolved: resolvedCounts
    };
  }

  initCharts() {
    if (!this.tickets || this.tickets.length === 0) return; // No inicializar si no hay datos
    const statusChartEl = document.getElementById('statusChart') as HTMLCanvasElement;
    if (statusChartEl) {
      if (this.statusChart) {
        this.statusChart.destroy();
      }
      this.statusChart = new Chart(statusChartEl, {
        type: 'doughnut',
        data: {
          labels: ['Abiertos', 'En Progreso', 'Pendientes', 'Resueltos', 'Urgentes'],
          datasets: [{
            data: [
              this.totalAbiertos,
              this.totalEnProgreso,
              this.totalPendientes,
              this.totalResueltos,
              this.totalUrgentes
            ],
            backgroundColor: [
              'rgba(59, 130, 246, 0.8)',
              'rgba(245, 158, 11, 0.8)',
              'rgba(139, 92, 246, 0.8)',
              'rgba(16, 185, 129, 0.8)',
              'rgba(239, 68, 68, 0.8)'
            ]
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false
        }
      });
    }
    // Tendencia Mensual (real)
    const trendChartEl = document.getElementById('trendChart') as HTMLCanvasElement;
    if (trendChartEl) {
      if (this.trendChart) {
        this.trendChart.destroy();
      }
      const stats = this.getMonthlyTicketStats();
      this.trendChart = new Chart(trendChartEl, {
        type: 'line',
        data: {
          labels: stats.labels,
          datasets: [
            {
              label: 'Tickets Creados',
              data: stats.created,
              borderColor: 'rgba(59, 130, 246, 1)',
              backgroundColor: 'rgba(59, 130, 246, 0.1)',
              tension: 0.4
            },
            {
              label: 'Tickets Resueltos',
              data: stats.resolved,
              borderColor: 'rgba(16, 185, 129, 1)',
              backgroundColor: 'rgba(16, 185, 129, 0.1)',
              tension: 0.4
            }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false
        }
      });
    }
  }

  cargarEstados() {
    this.http.get<EstadoTicket[]>('/api/tickets/estados').subscribe(estados => {
      this.estados = estados;
    });
  }

  cargarCategorias() {
    this.http.get<any[]>('/api/tickets/categorias').subscribe(data => {
      this.categorias = data;
    });
  }

  showSection(section: string) {
    this.currentSection = section;
    if (section === 'dashboard') {
      this.getTickets(); // Recarga datos y gráficos al entrar al dashboard
    }
    if (section === 'users') this.filterUsers();
    if (section === 'tickets') this.filterTickets();
  }

  // --- USUARIOS ---
  getUsers() {
    this.loadingUsers = true;
    this.usuarioService.getAll().subscribe({
      next: (data) => {
        this.users = data;
        this.updateRoleCounts();
        this.filterUsers();
        this.loadingUsers = false;
      },
      error: () => { this.userError = 'Error al cargar usuarios'; this.loadingUsers = false; }
    });
  }

  updateRoleCounts() {
    this.totalClientes = this.users.filter(u => u.tipoUsuario === 'Cliente').length;
    this.totalAgentes = this.users.filter(u => u.tipoUsuario === 'Agente').length;
    this.totalSupervisores = this.users.filter(u => u.tipoUsuario === 'Supervisor').length;
    this.totalAdministradores = this.users.filter(u => u.tipoUsuario === 'Administrador').length;
  }

  filterUsers() {
    const search = this.userSearch.toLowerCase();
    const role = this.userRoleFilter.toLowerCase();
    this.filteredUsers = this.users.filter(user => {
      const matchesSearch = user.nombreUsuario.toLowerCase().includes(search) || user.correoUsuario.toLowerCase().includes(search);
      const matchesRole = !role || (user.tipoUsuario && user.tipoUsuario.toLowerCase() === role);
      return matchesSearch && matchesRole;
    });
  }

  openUserModal() {
    this.userEditMode = false;
    this.selectedUser = null;
    this.formUser = { tipoUsuario: 'Cliente' };
    this.showUserModal = true;
  }
  closeUserModal() {
    this.userEditMode = false;
    this.selectedUser = null;
    this.showUserModal = false;
  }

  createUser() {
    const usuario: Usuario = {
      nombreUsuario: this.formUser.nombreUsuario!,
      correoUsuario: this.formUser.correoUsuario!,
      contrasenaUsuario: this.formUser.contrasenaUsuario!,
      tipoUsuario: this.formUser.tipoUsuario!,
      departamentoUsuario: this.formUser.departamentoUsuario || undefined,
      estadoUsuario: this.formUser.estadoUsuario || undefined
    };
    this.usuarioService.create(usuario).subscribe({
      next: () => { this.getUsers(); this.formUser = { tipoUsuario: 'Cliente' }; this.showUserModal = false; },
      error: (err) => {
        this.userError = err?.error?.message || 'Error al crear usuario';
      }
    });
  }

  startEditUser(user: Usuario) {
    this.selectedUser = { ...user };
    this.formUser = { ...user };
    this.userEditMode = true;
    this.showUserModal = true;
  }

  saveUserEdit() {
    if (!this.selectedUser) return;
    const usuario: Usuario = {
      nombreUsuario: this.formUser.nombreUsuario!,
      correoUsuario: this.formUser.correoUsuario!,
      contrasenaUsuario: this.formUser.contrasenaUsuario!,
      tipoUsuario: this.formUser.tipoUsuario!,
      departamentoUsuario: this.formUser.departamentoUsuario || undefined,
      estadoUsuario: this.formUser.estadoUsuario || undefined
    };
    this.usuarioService.update((this.selectedUser as any).idUsuario, usuario).subscribe({
      next: () => { this.getUsers(); this.userEditMode = false; this.selectedUser = null; this.showUserModal = false; },
      error: (err) => {
        this.userError = err?.error?.message || 'Error al actualizar usuario';
      }
    });
  }

  deleteUser(user: Usuario) {
    if (!confirm('¿Eliminar este usuario?')) return;
    this.usuarioService.delete((user as any).idUsuario).subscribe({
      next: () => this.getUsers(),
      error: (err) => { this.userError = err?.error?.message || 'Error al eliminar usuario'; }
    });
  }

  onRoleChange(event: any) {
    if (event.target.value === 'Cliente') {
      this.formUser.tipoUsuario = 'Cliente';
    }
  }

  openProfile() { this.showProfile = true; }
  closeProfile() { this.showProfile = false; }

  logout() {
    localStorage.clear();
    window.location.href = '/hogar';
  }

  showNotifications() {
    alert('Panel de notificaciones en desarrollo');
  }

  // --- TICKETS ---
  getTickets() {
    this.loadingTickets = true;
    // Primero cargar estados, luego tickets
    this.cargarEstados();
    this.ticketsService.getAll().subscribe({
      next: (data) => {
        this.tickets = data;
        // Espera a que los estados estén cargados antes de calcular contadores
        setTimeout(() => {
          console.log('Estados en tickets:', this.tickets.map(t => t.idEstadoTicket));
          console.log('Estados cargados:', this.estados);
          this.updateTicketCounts();
          this.filterTickets();
          this.loadingTickets = false;
          setTimeout(() => this.initCharts(), 0);
        }, 200); // Espera breve para asegurar que los estados estén cargados
      },
      error: () => { this.ticketError = 'Error al cargar tickets'; this.loadingTickets = false; }
    });
  }

  updateTicketCounts() {
    this.totalAbiertos = this.tickets.filter(t => this.getEstadoNombre(t.idEstadoTicket) === 'Abierto').length;
    this.totalEnProgreso = this.tickets.filter(t => this.getEstadoNombre(t.idEstadoTicket) === 'En Progreso').length;
    this.totalPendientes = this.tickets.filter(t => this.getEstadoNombre(t.idEstadoTicket) === 'Pendiente').length;
    this.totalResueltos = this.tickets.filter(t => this.getEstadoNombre(t.idEstadoTicket) === 'Resuelto').length;
    this.totalUrgentes = this.tickets.filter(t => t.prioridadTicket?.toLowerCase() === 'urgente').length;
    console.log('Contadores:', {
      abiertos: this.totalAbiertos,
      enProgreso: this.totalEnProgreso,
      pendientes: this.totalPendientes,
      resueltos: this.totalResueltos,
      urgentes: this.totalUrgentes,
      tickets: this.tickets.map(t => ({ id: t.idTicket, estado: this.getEstadoNombre(t.idEstadoTicket), prioridad: t.prioridadTicket }))
    });
  }

  filterTickets() {
    const search = this.ticketSearch.toLowerCase();
    const status = this.ticketStatusFilter.toLowerCase();
    const category = this.ticketCategoryFilter.toLowerCase();
    this.filteredTickets = this.tickets.filter(ticket => {
      const matchesSearch = ticket.tituloTicket.toLowerCase().includes(search) || ticket.descripcionTicket?.toLowerCase().includes(search) || ticket.idTicket.toString().includes(search);
      const matchesStatus = !status || this.getEstadoNombre(ticket.idEstadoTicket).toLowerCase() === status;
      const matchesCategory = !category || this.getCategoriaNombre(ticket.idCategoriaTicket).toLowerCase() === category;
      return matchesSearch && matchesStatus && matchesCategory;
    });
  }

  getEstadoNombre(idEstado: number): string {
    const estado = this.estados.find(e => e.idEstado === idEstado);
    if (!estado) return idEstado.toString();
    switch (estado.nombreEstado.trim().toLowerCase()) {
      case 'abierto': return 'Abierto';
      case 'en progreso': return 'En Progreso';
      case 'en-progreso': return 'En Progreso';
      case 'pendiente': return 'Pendiente';
      case 'resuelto': return 'Resuelto';
      case 'urgente': return 'Urgente';
      default: return estado.nombreEstado;
    }
  }
  getCategoriaNombre(idCategoria: number): string {
    const categoria = this.categorias.find(c => c.idCategoria === idCategoria);
    return categoria ? categoria.nombreCategoria : 'Otro';
  }

  cambiarEstadoTicket(ticket: Ticket, estado: string) {
    const estadoObj = this.estados.find(e => e.nombreEstado.toLowerCase() === estado.toLowerCase());
    if (!estadoObj) return;
    const actualizado = {
      ...ticket,
      idEstadoTicket: estadoObj.idEstado,
      tituloTicket: ticket.tituloTicket || '',
      descripcionTicket: ticket.descripcionTicket || '',
      idCliente: ticket.idCliente,
      idCategoriaTicket: ticket.idCategoriaTicket,
      prioridadTicket: ticket.prioridadTicket || 'media',
      ubicacionTicket: ticket.ubicacionTicket || '',
      departamentoTicket: ticket.departamentoTicket || '',
      fechaHoraCreacionTicket: ticket.fechaHoraCreacionTicket || '',
      fechaHoraActualizacionTicket: new Date().toISOString()
    };
    this.ticketsService.update(ticket.idTicket, actualizado).subscribe({
      next: () => {
        this.getTickets(); // Recarga la lista de tickets
        // Actualiza el ticket seleccionado si corresponde
        if (this.filteredTickets && this.filteredTickets.length > 0) {
          const idx = this.filteredTickets.findIndex(t => t.idTicket === ticket.idTicket);
          if (idx !== -1) {
            this.filteredTickets[idx].idEstadoTicket = estadoObj.idEstado;
          }
        }
      },
      error: (err) => { /* Manejo de error */ }
    });
  }

  finalizarTicket(ticket: Ticket) {
    this.cambiarEstadoTicket(ticket, 'Resuelto');
  }
  ponerPendiente(ticket: Ticket) {
    this.cambiarEstadoTicket(ticket, 'Pendiente');
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
        this.getTickets(); // Recarga la lista
      },
      error: (err) => { /* Manejo de error */ }
    });
  }
  ponerEnProgreso(ticket: Ticket) {
    this.cambiarEstadoTicket(ticket, 'En Progreso');
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
        this.getTickets();
      },
      error: err => {
        alert('Error al crear ticket: ' + JSON.stringify(err.error?.errors || err.error));
      }
    });
  }
}
