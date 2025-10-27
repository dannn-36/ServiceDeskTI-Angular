import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdministradorComponent } from './administrador/administrador.component';
import { AgenteComponent } from './agente/agente.component';
import { TicketsComponent } from './tickets/tickets.component';
import { HogarComponent } from './hogar/hogar.component';
import { SupervisorComponent } from './supervisor/supervisor.component';
import { EndUserComponent } from './end-user/end-user.component';

const routes: Routes = [
  { path: '', component: HogarComponent },
  { path: 'hogar', component: HogarComponent }, // Agrega la ruta /hogar para login
  { path: 'end-user', component: EndUserComponent },
  { path: 'administrador', component: AdministradorComponent },
  { path: 'agente', component: AgenteComponent },
  { path: 'tickets', component: TicketsComponent },
  { path: 'supervisor', component: SupervisorComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
