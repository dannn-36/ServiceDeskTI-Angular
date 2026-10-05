import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';

import { AppComponent } from './app.component';
import { HogarComponent } from './hogar/hogar.component';
import { AuthService } from './core/auth.service';
import { authInterceptor } from './core/auth.interceptor';

/** Recupera la sesión desde la cookie antes de evaluar la primera ruta. */
export function restaurarSesionAlArrancar(auth: AuthService): () => Promise<void> {
  return () => auth.restaurarSesion();
}

/**
 * Módulo raíz: solo el login y la infraestructura común.
 * Los paneles de cada rol son módulos con carga diferida (ver app-routing.module.ts).
 */
@NgModule({
  declarations: [AppComponent, HogarComponent],
  imports: [BrowserModule, FormsModule, AppRoutingModule],
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
    {
      provide: APP_INITIALIZER,
      useFactory: restaurarSesionAlArrancar,
      deps: [AuthService],
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
