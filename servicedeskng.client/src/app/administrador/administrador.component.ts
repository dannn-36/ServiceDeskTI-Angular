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
  formUser: Usuario = {
    nombreUsuario: '',
    correoUsuario: '',
    contrasenaUsuario: '',
    departamentoUsuario: '',
    estadoUsuario: 'activo',
    ubicacionUsuario: ''
  };
  tipoUsuario: string = 'EndUser'; // Solo para creación
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
    // No filtrar por tipoUsuario, solo por nombre/correo
    this.filteredUsers = this.users.filter(user => {
      const matchesSearch = user.nombreUsuario.toLowerCase().includes(search) || user.correoUsuario.toLowerCase().includes(search);
      return matchesSearch;
    });
  }

  openUserModal() {
    this.userEditMode = false;
    this.selectedUser = null;
    this.formUser = {
      nombreUsuario: '',
      correoUsuario: '',
      contrasenaUsuario: '',
      departamentoUsuario: '',
      estadoUsuario: 'activo',
      ubicacionUsuario: ''
    };
    this.tipoUsuario = 'EndUser';
    this.showUserModal = true;
  }
  closeUserModal() {
    this.userEditMode = false;
    this.selectedUser = null;
    this.showUserModal = false;
  }

  createUser() {
    this.usuarioService.create(this.formUser, this.tipoUsuario).subscribe({
      next: () => { this.getUsers(); this.openUserModal(); this.showUserModal = false; },
      error: (err) => {
        this.userError = err?.error?.message || 'Error al crear usuario';
      }
    });
  }

  startEditUser(user: Usuario) {
    this.selectedUser = { ...user };
    this.formUser = { ...user, contrasenaUsuario: '' };
    this.userEditMode = true;
    this.showUserModal = true;
  }

  saveUserEdit() {
    if (!this.selectedUser) return;
    this.usuarioService.update(this.selectedUser.idUsuario!, this.formUser).subscribe({
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
    // Vacío porque ya no se usa this.newUser.tipoUsuario
  }

  openProfile() { this.showProfile = true; }
  closeProfile() { this.showProfile = false; }
}
