import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ClienteComponent } from './cliente/cliente.component';
import { AdministradorComponent } from './administrador/administrador.component';
import { AgenteComponent } from './agente/agente.component';
import { TicketsComponent } from './tickets/tickets.component';
import { HogarComponent } from './hogar/hogar.component';
import { SupervisorComponent } from './supervisor/supervisor.component';

@NgModule({
  declarations: [
    AppComponent,
    ClienteComponent,
    AdministradorComponent,
    AgenteComponent,
    TicketsComponent,
    HogarComponent,
    SupervisorComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
