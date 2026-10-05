import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AdministradorComponent } from './administrador.component';
import { AuditoriaComponent } from '../auditoria/auditoria.component';

/** Panel de administración. Se descarga solo cuando entra un administrador. */
@NgModule({
  declarations: [AdministradorComponent, AuditoriaComponent],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild([{ path: '', component: AdministradorComponent }])
  ]
})
export class AdministradorModule { }
