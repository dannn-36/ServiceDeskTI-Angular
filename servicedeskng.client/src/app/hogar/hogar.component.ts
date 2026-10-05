import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';
import { mensajeDeError } from '../core/modelos';

@Component({
  selector: 'app-hogar',
  templateUrl: './hogar.component.html',
  styleUrls: ['./hogar.component.css']
})
export class HogarComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  correoUsuario = '';
  contrasenaUsuario = '';
  loginError = '';
  enviando = false;

  /**
   * El backend devuelve rol e identificadores en la misma respuesta del login,
   * así que ya no hace falta una segunda llamada para averiguar el idCliente/idAgente.
   */
  login(): void {
    if (this.enviando) {
      return;
    }

    this.loginError = '';
    this.enviando = true;

    this.auth.login(this.correoUsuario.trim(), this.contrasenaUsuario).subscribe({
      next: () => {
        this.enviando = false;
        this.contrasenaUsuario = '';
        void this.router.navigateByUrl(this.auth.rutaInicio());
      },
      error: err => {
        this.enviando = false;
        this.loginError = mensajeDeError(err, 'No se pudo iniciar sesión.');
      }
    });
  }
}
