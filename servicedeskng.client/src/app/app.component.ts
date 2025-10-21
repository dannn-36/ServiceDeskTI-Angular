import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'servicedeskng.client';

  public forecasts: WeatherForecast[] = [];
  correoUsuario: string = '';
  contrasenaUsuario: string = '';
  rol: string | null = null;
  loginError: string = '';
  showMenu = true;

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit() {
    this.rol = localStorage.getItem('rol');
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        // Oculta el menú solo en la página de login
        this.showMenu = !['/', '/hogar', '/login'].includes(this.router.url);
      }
    });
  }

  getForecasts() {
    this.http.get<WeatherForecast[]>('/weatherforecast').subscribe(
      (result) => {
        this.forecasts = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }

  login() {
    this.loginError = '';
    const body = {
      CorreoUsuario: this.correoUsuario,
      ContrasenaUsuario: this.contrasenaUsuario
    };
    this.http.post<any>('/api/auth/login', body).subscribe({
      next: (response) => {
        this.rol = response.rol;
        localStorage.setItem('rol', response.rol);
        // Puedes guardar más datos si lo necesitas
      },
      error: (err) => {
        this.loginError = err.error?.message || 'Error de autenticación';
      }
    });
  }

  logout() {
    localStorage.removeItem('rol');
    this.rol = null;
    this.correoUsuario = '';
    this.contrasenaUsuario = '';
  }
}
