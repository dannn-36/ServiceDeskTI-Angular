import { HttpErrorResponse } from '@angular/common/http';
import { esEstadoActivo, esEstadoFinalizado, mensajeDeError, mensajeDeErrorBlob, normalizarEstado } from './modelos';

describe('Utilidades de modelos', () => {
  it('normaliza las variantes de nombre de estado que conviven en la base', () => {
    expect(normalizarEstado('En Progreso')).toBe('en-progreso');
    expect(normalizarEstado(' en-progreso ')).toBe('en-progreso');
    expect(normalizarEstado(undefined)).toBe('');
  });

  it('clasifica estados activos y finalizados', () => {
    expect(esEstadoActivo('Abierto')).toBeTrue();
    expect(esEstadoActivo('En Progreso')).toBeTrue();
    expect(esEstadoActivo('Resuelto')).toBeFalse();
    expect(esEstadoFinalizado('Resuelto')).toBeTrue();
  });

  it('extrae el mensaje { message } que devuelve la API', () => {
    const error = new HttpErrorResponse({ status: 409, error: { message: 'Ya existe un usuario con ese correo.' } });
    expect(mensajeDeError(error, 'genérico')).toBe('Ya existe un usuario con ese correo.');
  });

  it('usa el mensaje por defecto si el servidor no responde', () => {
    expect(mensajeDeError(new HttpErrorResponse({ status: 0 }), 'genérico'))
      .toBe('No se pudo contactar con el servidor.');
    expect(mensajeDeError(new Error('x'), 'genérico')).toBe('genérico');
  });

  it('lee el mensaje de error cuando la respuesta era un Blob (descargas)', async () => {
    const cuerpo = new Blob([JSON.stringify({ message: 'No se encontró mysqldump.' })], { type: 'application/json' });
    const error = new HttpErrorResponse({ status: 409, error: cuerpo });

    expect(await mensajeDeErrorBlob(error, 'genérico')).toBe('No se encontró mysqldump.');
  });
});
