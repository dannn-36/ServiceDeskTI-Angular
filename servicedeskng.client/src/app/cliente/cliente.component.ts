import { Component } from '@angular/core';

@Component({
  selector: 'app-cliente',
  templateUrl: './cliente.component.html',
  styleUrls: ['./cliente.component.css']
})
export class ClienteComponent {
  cliente = {
    idCliente: 0,
    idUsuario: 0,
    idNivel: 0
  };
}
