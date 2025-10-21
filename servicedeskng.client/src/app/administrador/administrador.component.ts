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
  newUser: Partial<Usuario> = { tipoUsuario: 'EndUser' };
  userEditMode = false;

  // Secciones y perfil
  currentSection: string = 'dashboard';
  showProfile = false;
  profileName = 'Admin Sistema';
  profileEmail = 'admin@empresa.com';

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
      next: (data) => { this.users = data; this.filterUsers(); this.loadingUsers = false; },
      error: () => { this.userError = 'Error al cargar usuarios'; this.loadingUsers = false; }
    });
  }

  filterUsers() {
    const search = this.userSearch.toLowerCase();
    const role = this.userRoleFilter.toLowerCase();
    this.filteredUsers = this.users.filter(user => {
      const matchesSearch = user.nombreUsuario.toLowerCase().includes(search) || user.correoUsuario.toLowerCase().includes(search);
      const matchesRole = !role || user.tipoUsuario.toLowerCase() === role;
      return matchesSearch && matchesRole;
    });
  }

  openUserModal() {
    this.userEditMode = false;
    this.selectedUser = null;
    this.newUser = { tipoUsuario: 'EndUser' };
    this.showUserModal = true;
  }
  closeUserModal() {
    this.userEditMode = false;
    this.selectedUser = null;
    this.showUserModal = false;
  }

  createUser() {
    // Solo enviar los campos requeridos
    const usuario: Usuario = {
      nombreUsuario: this.newUser.nombreUsuario!,
      correoUsuario: this.newUser.correoUsuario!,
      contrasenaUsuario: this.newUser.contrasenaUsuario!,
      tipoUsuario: this.newUser.tipoUsuario!,
      departamentoUsuario: this.newUser.departamentoUsuario || undefined,
      estadoUsuario: this.newUser.estadoUsuario || undefined
    };
    this.usuarioService.create(usuario).subscribe({
      next: () => { this.getUsers(); this.newUser = { tipoUsuario: 'EndUser' }; this.showUserModal = false; },
      error: (err) => {
        this.userError = err?.error?.message || 'Error al crear usuario';
      }
    });
  }

  startEditUser(user: Usuario) {
    this.selectedUser = { ...user };
    this.userEditMode = true;
    this.showUserModal = true;
  }

  saveUserEdit() {
    if (!this.selectedUser) return;
    const usuario: Usuario = {
      nombreUsuario: this.selectedUser.nombreUsuario!,
      correoUsuario: this.selectedUser.correoUsuario!,
      contrasenaUsuario: this.selectedUser.contrasenaUsuario!,
      tipoUsuario: this.selectedUser.tipoUsuario!,
      departamentoUsuario: this.selectedUser.departamentoUsuario || undefined,
      estadoUsuario: this.selectedUser.estadoUsuario || undefined
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
    // Si el usuario selecciona "Cliente", forzar el valor a "EndUser"
    if (event.target.value === 'Cliente') {
      this.newUser.tipoUsuario = 'EndUser';
    }
  }

  openProfile() { this.showProfile = true; }
  closeProfile() { this.showProfile = false; }
}
