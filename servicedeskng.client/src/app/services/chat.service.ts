import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ChatService {

  private hubConnection!: signalR.HubConnection;
  private readonly hubUrl = 'https://localhost:5076/chathub'; // Ajusta al puerto real del backend

  constructor() { }

  // 🔹 Conectar al hub
  connect(ticketId: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('✅ Conectado al hub');
        this.joinTicket(ticketId);
      })
      .catch(err => console.error('❌ Error de conexión:', err));
  }

  // 🔹 Unirse al grupo del ticket
  joinTicket(ticketId: string): void {
    this.hubConnection.invoke('JoinTicket', ticketId);
  }

  // 🔹 Salir del grupo
  leaveTicket(ticketId: string): void {
    this.hubConnection.invoke('LeaveTicket', ticketId);
  }

  // 🔹 Escuchar mensajes entrantes
  onReceiveMessage(callback: (user: string, message: string, fecha: string) => void): void {
    this.hubConnection.on('ReceiveMessage', (data) => {
      callback(data.usuario, data.mensaje, data.fecha);
    });
  }

  // 🔹 Enviar mensaje
  sendMessage(ticketId: string, userName: string, message: string, userId: number): void {
    this.hubConnection.invoke('SendMessage', ticketId, userName, message, userId)
      .catch(err => console.error('Error al enviar mensaje:', err));
  }

  // 🔹 Cerrar conexión
  disconnect(ticketId: string): void {
    this.leaveTicket(ticketId);
    this.hubConnection.stop();
  }
}
