import { Component } from '@angular/core';

@Component({
  selector: 'app-tickets',
  templateUrl: './tickets.component.html',
  styleUrls: ['./tickets.component.css']
})
export class TicketsComponent {
  ticket = {
    idTicket: 0,
    idCliente: 0,
    idAgenteAsignado: 0,
    idEstadoTicket: 0,
    idCategoriaTicket: 0,
    tituloTicket: '',
    descripcionTicket: '',
    prioridadTicket: '',
    ubicacionTicket: '',
    departamentoTicket: '',
    fechaHoraCreacionTicket: '',
    fechaHoraActualizacionTicket: ''
  };
}
