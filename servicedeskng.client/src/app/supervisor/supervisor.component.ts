import { Component } from '@angular/core';

@Component({
  selector: 'app-supervisor',
  templateUrl: './supervisor.component.html',
  styleUrls: ['./supervisor.component.css']
})
export class SupervisorComponent {
  supervisor = {
    idSupervisor: 0,
    idUsuario: 0,
    idNivel: 0
  };
}
