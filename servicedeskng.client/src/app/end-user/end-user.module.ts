import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { EndUserComponent } from './end-user.component';

/** Portal del cliente. Se descarga solo cuando entra un cliente. */
@NgModule({
  declarations: [EndUserComponent],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild([{ path: '', component: EndUserComponent }])
  ]
})
export class EndUserModule { }
