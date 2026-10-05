import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HogarComponent } from './hogar/hogar.component';
import { invitadoGuard, rolGuard } from './core/auth.guard';

/**
 * Cada panel exige su rol y se carga de forma diferida:
 * - Antes cualquiera podía escribir /administrador en la barra de direcciones y entrar.
 * - Con `canMatch`, el código de un panel ni siquiera se descarga si el rol no corresponde,
 *   y la pantalla de login no arrastra Chart.js ni SignalR.
 * (La API valida el rol igualmente en cada petición.)
 */
const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'hogar' },
  { path: 'hogar', component: HogarComponent, canActivate: [invitadoGuard] },
  {
    path: 'end-user',
    canMatch: [rolGuard('Cliente')],
    loadChildren: () => import('./end-user/end-user.module').then(m => m.EndUserModule)
  },
  {
    path: 'agente',
    canMatch: [rolGuard('Agente')],
    loadChildren: () => import('./agente/agente.module').then(m => m.AgenteModule)
  },
  {
    path: 'supervisor',
    canMatch: [rolGuard('Supervisor')],
    loadChildren: () => import('./supervisor/supervisor.module').then(m => m.SupervisorModule)
  },
  {
    path: 'administrador',
    canMatch: [rolGuard('Administrador')],
    loadChildren: () => import('./administrador/administrador.module').then(m => m.AdministradorModule)
  },
  { path: '**', redirectTo: 'hogar' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
