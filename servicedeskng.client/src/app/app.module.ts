import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AppRoutingModule } from './app-routing.module';

import { AppComponent } from './app.component';
import { AdministradorComponent } from './administrador/administrador.component';
import { AgenteComponent } from './agente/agente.component';
import { TicketsComponent } from './tickets/tickets.component';
import { HogarComponent } from './hogar/hogar.component';
import { SupervisorComponent } from './supervisor/supervisor.component';
import { UsuarioComponent } from './usuario/usuario.component';
import { EndUserComponent } from './end-user/end-user.component';

@NgModule({
  declarations: [
    AppComponent,
    AdministradorComponent,
    AgenteComponent,
    TicketsComponent,
    HogarComponent,
    SupervisorComponent,
    UsuarioComponent,
    EndUserComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    CommonModule,
    AppRoutingModule,
    RouterModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
