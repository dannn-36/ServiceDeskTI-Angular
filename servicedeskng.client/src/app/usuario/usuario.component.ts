import { Component, OnInit } from '@angular/core';
import { UsuarioService, Usuario } from './usuario.service';

@Component({
  selector: 'app-usuario',
  templateUrl: './usuario.component.html',
  styleUrls: ['./usuario.component.css']
})
export class UsuarioComponent implements OnInit {
  usuarios: Usuario[] = [];
  loading = false;
  error = '';

  constructor(private usuarioService: UsuarioService) {}

  ngOnInit(): void {
    this.getUsuarios();
  }

  getUsuarios() {
    this.loading = true;
    this.usuarioService.getAll().subscribe({
      next: (data) => { this.usuarios = data; this.loading = false; },
      error: (err) => { this.error = err?.error?.message || 'Error al cargar usuarios'; this.loading = false; }
    });
  }
}
