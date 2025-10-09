import { Component } from '@angular/core';

@Component({
  selector: 'app-administrador',
  templateUrl: './administrador.component.html',
  styleUrls: ['./administrador.component.css']
})
export class AdministradorComponent {
  administrador = {
    idAdmin: 0,
    idUsuario: 0,
    idNivel: 0,
    areaResponsabilidadAdmin: ''
  };
}
