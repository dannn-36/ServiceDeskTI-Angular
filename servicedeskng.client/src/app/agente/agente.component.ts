import { Component } from '@angular/core';

@Component({
  selector: 'app-agente',
  templateUrl: './agente.component.html',
  styleUrls: ['./agente.component.css']
})
export class AgenteComponent {
  agente = {
    idAgente: 0,
    idUsuario: 0,
    idNivel: 0,
    especialidadAgente: '',
    disponibilidadAgente: false
  };
}
