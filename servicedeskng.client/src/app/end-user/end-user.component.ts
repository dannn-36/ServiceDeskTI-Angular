import { Component, OnInit } from '@angular/core';
import { EndUserService, EndUser } from './end-user.service';

@Component({
  selector: 'app-end-user',
  templateUrl: './end-user.component.html',
  styleUrls: ['./end-user.component.css']
})
export class EndUserComponent implements OnInit {
  users: EndUser[] = [];
  filteredUsers: EndUser[] = [];
  userSearch: string = '';
  userRoleFilter: string = '';
  loading = false;
  error = '';
  selectedUser: EndUser | null = null;
  newUser: Partial<EndUser> = {};
  editMode = false;
  showUserModal = false;

  constructor(private endUserService: EndUserService) {}

  ngOnInit() {
    this.getUsers();
  }

  getUsers() {
    this.loading = true;
    this.endUserService.getAll().subscribe({
      next: (data) => { this.users = data; this.filterUsers(); this.loading = false; },
      error: () => { this.error = 'Error al cargar usuarios'; this.loading = false; }
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
    this.editMode = false;
  }

  startEdit(user: EndUser) {
    this.selectedUser = { ...user };
    this.editMode = true;
  }

  saveEdit() {
    if (!this.selectedUser) return;
    this.endUserService.update(this.selectedUser.idEndUser, this.selectedUser).subscribe({
      next: () => { this.getUsers(); this.editMode = false; this.selectedUser = null; },
      error: () => { this.error = 'Error al actualizar usuario'; }
    });
  }

  deleteUser(user: EndUser) {
    if (!confirm('¿Eliminar este usuario?')) return;
    this.endUserService.delete(user.idEndUser).subscribe({
      next: () => this.getUsers(),
      error: () => { this.error = 'Error al eliminar usuario'; }
    });
  }

  createUser() {
    this.endUserService.create(this.newUser as EndUser).subscribe({
      next: () => { this.getUsers(); this.newUser = {}; },
      error: () => { this.error = 'Error al crear usuario'; }
    });
  }

  cancelEdit() {
    this.editMode = false;
    this.selectedUser = null;
  }

  openUserModal() { this.showUserModal = true; }
  closeUserModal() { this.showUserModal = false; }
}
