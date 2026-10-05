import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';
import { MensajeChatEnVivo, MensajeTicket } from '../core/modelos';

/**
 * Chat en tiempo real de un ticket.
 *
 * - Usa rutas relativas (/chathub), que el proxy de desarrollo reenvía al backend
 *   y que en producción apuntan al mismo servidor. Antes estaba fija a localhost:5076.
 * - La autenticación viaja en la cookie de sesión: el hub ya no recibe
 *   userId ni userName del cliente, así que nadie puede escribir como otra persona.
 * - Los mensajes se exponen como Observable; cada componente se suscribe y se
 *   desuscribe al destruirse. Antes se registraba un manejador nuevo en cada apertura
 *   del chat y los mensajes llegaban duplicados.
 */
@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly http = inject(HttpClient);

  private conexion: signalR.HubConnection | null = null;
  private ticketActual: number | null = null;
  private readonly mensajesEnVivo = new Subject<MensajeChatEnVivo>();

  /** Mensajes que llegan por el hub para el ticket conectado. */
  readonly mensajes$: Observable<MensajeChatEnVivo> = this.mensajesEnVivo.asObservable();

  historial(idTicket: number): Observable<MensajeTicket[]> {
    return this.http.get<MensajeTicket[]>(`/api/TicketMensaje/ticket/${idTicket}`);
  }

  async conectar(idTicket: number): Promise<void> {
    await this.desconectar();

    const conexion = new signalR.HubConnectionBuilder()
      .withUrl('/chathub')
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    conexion.on('ReceiveMessage', (mensaje: MensajeChatEnVivo) => {
      if (mensaje.idTicket === this.ticketActual) {
        this.mensajesEnVivo.next(mensaje);
      }
    });

    // Tras una reconexión el servidor ya no recuerda el grupo: hay que volver a unirse.
    conexion.onreconnected(() => conexion.invoke('JoinTicket', String(idTicket)));

    this.conexion = conexion;
    this.ticketActual = idTicket;

    await conexion.start();
    await conexion.invoke('JoinTicket', String(idTicket));
  }

  async enviar(texto: string): Promise<void> {
    const mensaje = texto.trim();
    if (!mensaje) {
      return;
    }

    if (!this.conexion || this.ticketActual === null
        || this.conexion.state !== signalR.HubConnectionState.Connected) {
      throw new Error('No hay conexión con el chat. Vuelve a abrir el ticket.');
    }

    await this.conexion.invoke('SendMessage', String(this.ticketActual), mensaje);
  }

  async desconectar(): Promise<void> {
    const conexion = this.conexion;
    const ticket = this.ticketActual;

    this.conexion = null;
    this.ticketActual = null;

    if (!conexion) {
      return;
    }

    try {
      if (ticket !== null && conexion.state === signalR.HubConnectionState.Connected) {
        await conexion.invoke('LeaveTicket', String(ticket));
      }
    } finally {
      await conexion.stop();
    }
  }
}
