import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-hogar',
  templateUrl: './hogar.component.html',
  styleUrls: ['./hogar.component.css']
})
export class HogarComponent {
  correoUsuario: string = '';
  contrasenaUsuario: string = '';
  loginError: string = '';

  constructor(private http: HttpClient, private router: Router) {}

  login() {
    this.loginError = '';
    const body = {
      CorreoUsuario: this.correoUsuario,
      ContrasenaUsuario: this.contrasenaUsuario
    };
    this.http.post<any>('/api/auth/login', body).subscribe({
      next: (response) => {
        console.log('Respuesta del backend:', response); // Para depuración
        localStorage.setItem('rol', response.rol);
        localStorage.setItem('usuarioId', response.usuario.idUsuario); // Guarda el idUsuario
        localStorage.setItem('usuario', response.usuario.nombreUsuario); // Guarda el nombre del usuario

        // Solo buscar clienteId si el rol es Cliente o EndUser
        if (response.rol === 'Cliente' || response.rol === 'EndUser') {
          this.http.get<any>(`/api/enduser/by-usuario/${response.usuario.idUsuario}`).subscribe({
            next: cliente => {
              localStorage.setItem('clienteId', cliente.idCliente);
              this.router.navigate(['/end-user']);
            },
            error: () => {
              // Si no existe cliente, igual redirige
              this.router.navigate(['/end-user']);
            }
          });
        } else if (response.rol === 'Administrador') {
          this.router.navigate(['/administrador']);
        } else if (response.rol === 'Agente') {
          this.router.navigate(['/agente']);
        } else if (response.rol === 'Supervisor') {
          this.router.navigate(['/supervisor']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.loginError = err.error?.message || 'Error de autenticación';
      }
    });
  }
}
