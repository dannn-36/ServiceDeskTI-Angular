import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AgenteComponent } from './agente.component';
import { TicketFiltroPipe } from './ticketFiltro.pipe';

/** Vista de agente. Se descarga solo cuando entra un agente. */
@NgModule({
  declarations: [AgenteComponent, TicketFiltroPipe],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild([{ path: '', component: AgenteComponent }])
  ]
})
export class AgenteModule { }
