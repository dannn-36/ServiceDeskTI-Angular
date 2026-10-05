import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SupervisorComponent } from './supervisor.component';

/** Panel de supervisión. Se descarga solo cuando entra un supervisor. */
@NgModule({
  declarations: [SupervisorComponent],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild([{ path: '', component: SupervisorComponent }])
  ]
})
export class SupervisorModule { }
