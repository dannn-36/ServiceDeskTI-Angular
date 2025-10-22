import { Component, OnInit } from '@angular/core';
import { UsuarioService, Usuario } from '../usuario/usuario.service';

@Component({
  selector: 'app-administrador',
  templateUrl: './administrador.component.html',
  styleUrls: ['./administrador.component.css']
})
export class AdministradorComponent implements OnInit {
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

  constructor(private usuarioService: UsuarioService) { }

  ngOnInit(): void {
    this.getUsers();
    this.filterUsers();
  }

  showSection(section: string) {
    this.currentSection = section;
    if (section === 'users') this.filterUsers();
  }

   

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
}
