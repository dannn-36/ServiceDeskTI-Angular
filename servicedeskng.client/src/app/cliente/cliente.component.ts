import { Component, OnInit } from '@angular/core';
import { ClienteService } from './cliente.service';

@Component({
  selector: 'app-cliente',
  templateUrl: './cliente.component.html', // debe existir este archivo
  styleUrls: ['./cliente.component.css']   // debe existir este archivo
})
export class ClienteComponent implements OnInit {

  clientes: any[] = [];
  clienteSeleccionado: any = null;

  constructor(private clienteService: ClienteService) { }

  ngOnInit(): void {
    this.cargarClientes();
  }

  cargarClientes(): void {
    this.clienteService.getAll().subscribe({
      next: data => this.clientes = data,
      error: err => console.error('Error al cargar clientes', err)
    });
  }

  seleccionarCliente(cliente: any): void {
    this.clienteSeleccionado = { ...cliente };
  }

  guardarCliente(cliente: any): void {
    if (cliente.idCliente && cliente.idCliente > 0) {
      this.clienteService.update(cliente.idCliente, cliente).subscribe({
        next: () => this.cargarClientes(),
        error: err => console.error('Error al actualizar cliente', err)
      });
    } else {
      this.clienteService.create(cliente).subscribe({
        next: () => this.cargarClientes(),
        error: err => console.error('Error al crear cliente', err)
      });
    }
    this.clienteSeleccionado = null;
  }

  eliminarCliente(id: number): void {
    this.clienteService.delete(id).subscribe({
      next: () => this.cargarClientes(),
      error: err => console.error('Error al eliminar cliente', err)
    });
  }

  nuevoCliente(): void {
    this.clienteSeleccionado = { idCliente: 0, idUsuario: 0, idNivel: 0 };
  }

  limpiarSeleccion(): void {
    this.clienteSeleccionado = null;
  }

  currentSection = 'dashboard';

  showSection(section: string, event: Event) {
    this.currentSection = section;
    // Opcional: manejo visual si quieres agregar clases activas en el sidebar
    event.preventDefault();
  }

  showNotifications() {
    // Implementar lógica o abrir modal de notificaciones
  }

  showProfileModal() {
    // Abrir modal perfil
  }

  confirmLogout() {
    if (confirm('¿Estás seguro de que deseas cerrar sesión?')) {
      // Aquí iría la lógica para cerrar sesión y redirigir
    }
  }
}

