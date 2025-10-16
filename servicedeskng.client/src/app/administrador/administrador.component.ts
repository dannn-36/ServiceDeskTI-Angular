import { Component, OnInit } from '@angular/core';
import { AdministradorService, Administrador } from './administrador.service';
import { TicketsService, Ticket } from '../tickets/tickets.service';
import { EndUserService, EndUser } from '../end-user/end-user.service';

@Component({
  selector: 'app-administrador',
  templateUrl: './administrador.component.html',
  styleUrls: ['./administrador.component.css']
})
export class AdministradorComponent implements OnInit {
  currentSection: string = 'dashboard';

  // Usuarios
  users: EndUser[] = [];
  filteredUsers: EndUser[] = [];
  userSearch: string = '';
  userRoleFilter: string = '';
  showUserModal = false;
  loadingUsers = false;
  userError = '';
  selectedUser: EndUser | null = null;
  newUser: Partial<EndUser> = {};
  userEditMode = false;

  // Tickets
  tickets: Ticket[] = [];
  filteredTickets: Ticket[] = [];
  ticketSearch: string = '';
  ticketStatusFilter: string = '';
  ticketCategoryFilter: string = '';
  loadingTickets = false;
  ticketError = '';
  selectedTicket: Ticket | null = null;
  newTicket: Partial<Ticket> = {};
  editMode = false;

  // Modales y perfil
  showProfile = false;
  profileName = 'Admin Sistema';
  profileEmail = 'admin@empresa.com';

  // Administradores (backend)
  administradores: Administrador[] = [];

  constructor(
    private administradorService: AdministradorService,
    private ticketsService: TicketsService,
    private endUserService: EndUserService
  ) { }

  ngOnInit(): void {
    this.loadAdministradores();
    this.getUsers();
    this.getTickets();
    this.filterUsers();
    this.filterTickets();
  }

  // --- Secciones ---
  showSection(section: string) {
    this.currentSection = section;
    if (section === 'users') this.filterUsers();
    if (section === 'tickets') this.filterTickets();
  }

  // --- Usuarios Backend ---
  getUsers() {
    this.loadingUsers = true;
    this.endUserService.getAll().subscribe({
      next: (data) => { this.users = data; this.filterUsers(); this.loadingUsers = false; },
      error: () => { this.userError = 'Error al cargar usuarios'; this.loadingUsers = false; }
    });
  }

  filterUsers() {
    const search = this.userSearch.toLowerCase();
    const role = this.userRoleFilter.toLowerCase();
    this.filteredUsers = this.users.filter(user => {
      const matchesSearch = user.nombre.toLowerCase().includes(search) || user.correo.toLowerCase().includes(search);
      const matchesRole = !role || user.rol.toLowerCase() === role;
      return matchesSearch && matchesRole;
    });
  }

  selectUser(user: EndUser) {
    this.selectedUser = { ...user };
    this.userEditMode = false;
    this.showUserModal = true;
  }

  startEditUser(user: EndUser) {
    this.selectedUser = { ...user };
    this.userEditMode = true;
    this.showUserModal = true;
  }

  saveUserEdit() {
    if (!this.selectedUser) return;
    this.endUserService.update(this.selectedUser.idEndUser, this.selectedUser).subscribe({
      next: () => { this.getUsers(); this.userEditMode = false; this.selectedUser = null; this.showUserModal = false; },
      error: () => { this.userError = 'Error al actualizar usuario'; }
    });
  }

  deleteUser(user: EndUser) {
    if (!confirm('¿Eliminar este usuario?')) return;
    this.endUserService.delete(user.idEndUser).subscribe({
      next: () => this.getUsers(),
      error: () => { this.userError = 'Error al eliminar usuario'; }
    });
  }

  createUser() {
    this.endUserService.create(this.newUser as EndUser).subscribe({
      next: () => { this.getUsers(); this.newUser = {}; this.showUserModal = false; },
      error: () => { this.userError = 'Error al crear usuario'; }
    });
  }

  cancelUserEdit() {
    this.userEditMode = false;
    this.selectedUser = null;
    this.showUserModal = false;
  }

  openUserModal() {
    this.userEditMode = false;
    this.selectedUser = null;
    this.showUserModal = true;
  }
  closeUserModal() {
    this.userEditMode = false;
    this.selectedUser = null;
    this.showUserModal = false;
  }

  // --- Tickets Backend ---
  getTickets() {
    this.loadingTickets = true;
    this.ticketsService.getAll().subscribe({
      next: (data) => { this.tickets = data; this.filterTickets(); this.loadingTickets = false; },
      error: (err) => { this.ticketError = 'Error al cargar tickets'; this.loadingTickets = false; }
    });
  }

  filterTickets() {
    const search = this.ticketSearch.toLowerCase();
    const status = this.ticketStatusFilter.toLowerCase();
    const category = this.ticketCategoryFilter.toLowerCase();
    this.filteredTickets = this.tickets.filter(ticket => {
      const matchesSearch = ticket.tituloTicket?.toLowerCase().includes(search) || ticket.idTicket?.toString().includes(search);
      const matchesStatus = !status || ticket.idEstadoTicket?.toString() === status;
      const matchesCategory = !category || ticket.idCategoriaTicket?.toString() === category;
      return matchesSearch && matchesStatus && matchesCategory;
    });
  }

  selectTicket(ticket: Ticket) {
    this.selectedTicket = { ...ticket };
    this.editMode = false;
  }

  startEdit(ticket: Ticket) {
    this.selectedTicket = { ...ticket };
    this.editMode = true;
  }

  saveEdit() {
    if (!this.selectedTicket) return;
    this.ticketsService.update(this.selectedTicket.idTicket, this.selectedTicket).subscribe({
      next: () => { this.getTickets(); this.editMode = false; this.selectedTicket = null; },
      error: () => { this.ticketError = 'Error al actualizar ticket'; }
    });
  }

  deleteTicket(ticket: Ticket) {
    if (!confirm('¿Eliminar este ticket?')) return;
    this.ticketsService.delete(ticket.idTicket).subscribe({
      next: () => this.getTickets(),
      error: () => { this.ticketError = 'Error al eliminar ticket'; }
    });
  }

  createTicket() {
    this.ticketsService.create(this.newTicket as Ticket).subscribe({
      next: () => { this.getTickets(); this.newTicket = {}; },
      error: () => { this.ticketError = 'Error al crear ticket'; }
    });
  }

  cancelEdit() {
    this.editMode = false;
    this.selectedTicket = null;
  }

  // --- Administradores Backend ---
  loadAdministradores() {
    this.administradorService.getAll().subscribe(data => {
      this.administradores = data;
    });
  }

  // --- Métodos de UI para modales y perfil ---
  openProfile() { this.showProfile = true; }
  closeProfile() { this.showProfile = false; }
}
