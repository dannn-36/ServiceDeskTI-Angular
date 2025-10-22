import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private hubConnection: HubConnection | null = null;
  private mensajesSubject = new Subject<{ remitente: string, texto: string, esCliente: boolean }>();
  mensajes$ = this.mensajesSubject.asObservable();

  conectar(ticketId: number) {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/chatHub?ticketId=' + ticketId)
      .build();
    this.hubConnection.on('RecibirMensaje', (remitente: string, texto: string, esCliente: boolean) => {
      this.mensajesSubject.next({ remitente, texto, esCliente });
    });
    this.hubConnection.start();
  }

  enviarMensaje(ticketId: number, remitente: string, texto: string, esCliente: boolean) {
    if (this.hubConnection) {
      this.hubConnection.invoke('EnviarMensaje', ticketId, remitente, texto, esCliente);
    }
  }

  desconectar() {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }
  }
}
