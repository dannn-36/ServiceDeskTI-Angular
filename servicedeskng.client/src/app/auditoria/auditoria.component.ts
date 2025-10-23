import { Component } from '@angular/core';

@Component({
  selector: 'app-auditoria',
  templateUrl: './auditoria.component.html',
  styleUrls: ['./auditoria.component.css']
})
export class AuditoriaComponent {
  auditoria = {
    idAuditoria: 0,
    idUsuario: 0,
    detalleAuditoria: '',
    fechaAuditoria: null
  };
}
