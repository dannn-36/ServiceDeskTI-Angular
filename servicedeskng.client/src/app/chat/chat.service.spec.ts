import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ChatService } from './chat.service';

describe('ChatService', () => {
  let servicio: ChatService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    servicio = TestBed.inject(ChatService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('pide el historial con una ruta relativa (sin localhost:5076 fijo)', () => {
    servicio.historial(42).subscribe();

    const peticion = http.expectOne('/api/TicketMensaje/ticket/42');
    expect(peticion.request.method).toBe('GET');
    peticion.flush([]);
  });

  it('no permite enviar si no hay conexión abierta', async () => {
    await expectAsync(servicio.enviar('hola')).toBeRejectedWithError(/No hay conexión/);
  });

  it('ignora mensajes vacíos sin intentar enviarlos', async () => {
    await expectAsync(servicio.enviar('   ')).toBeResolved();
  });
});
