import { HttpErrorResponse } from '@angular/common/http';

/** Roles tal como los emite el backend en /api/auth/login y /api/auth/me. */
export type Rol = 'Administrador' | 'Supervisor' | 'Agente' | 'Cliente';

/** Identidad de la sesión actual. Vive solo en memoria: la credencial es una cookie HttpOnly. */
export interface SesionUsuario {
  idUsuario: number;
  nombreUsuario: string;
  correoUsuario: string;
  rol: Rol;
  idCliente?: number | null;
  idAgente?: number | null;
  idSupervisor?: number | null;
  idAdministrador?: number | null;
}

/** Pantalla de inicio de cada rol. */
export const RUTA_POR_ROL: Record<Rol, string> = {
  Administrador: '/administrador',
  Supervisor: '/supervisor',
  Agente: '/agente',
  Cliente: '/end-user'
};

export interface EstadoTicket {
  idEstado: number;
  nombreEstado: string;
}

export interface CategoriaTicket {
  idCategoria: number;
  nombreCategoria: string;
}

/** Mensaje guardado en el historial de un ticket. */
export interface MensajeTicket {
  idMensaje: number;
  idTicket: number;
  idUsuario: number;
  mensajeTicket: string;
  fechaHoraCreacionMensaje?: string;
  usuarioNombre: string;
}

/** Mensaje recibido en vivo por el hub de SignalR. */
export interface MensajeChatEnVivo {
  idMensaje: number;
  idTicket: number;
  idUsuario: number;
  usuario: string;
  mensaje: string;
  fecha?: string;
}

export interface RegistroAuditoria {
  idAuditoria: number;
  idUsuario: number;
  nombreUsuario: string;
  accionAuditoria: string;
  detalleAuditoria?: string | null;
  fechaAuditoria?: string;
}

export interface ComparativaAgente {
  name: string;
  asignados: number;
  resueltos: number;
  activos: number;
  tasaResolucion: number;
  tiempoPromedioHoras: number | null;
}

export interface RendimientoSemanal {
  labels: string[];
  created: number[];
  resolved: number[];
}

export interface ClienteResumen {
  idCliente: number;
  idUsuario: number;
  nombreUsuario: string;
  correoUsuario: string;
}

/** Prioridades admitidas por la base de datos (ENUM prioridad_ticket). */
export const PRIORIDADES = ['baja', 'media', 'alta', 'urgente'] as const;
export type Prioridad = (typeof PRIORIDADES)[number];

/**
 * Normaliza el nombre de un estado para compararlo sin depender de
 * mayúsculas ni de si en la base se escribió "En Progreso" o "en-progreso".
 */
export function normalizarEstado(nombre: string | null | undefined): string {
  return (nombre ?? '').trim().toLowerCase().replace(/\s+/g, '-');
}

/** Estados (de Database/DatabaseScript.txt) en los que el ticket sigue abierto. */
export function esEstadoActivo(nombre: string | null | undefined): boolean {
  return ['abierto', 'en-progreso', 'pendiente', 'pendiente-usuario', 'reabierto']
    .includes(normalizarEstado(nombre));
}

export function esEstadoFinalizado(nombre: string | null | undefined): boolean {
  return ['resuelto', 'cerrado'].includes(normalizarEstado(nombre));
}

/** Extrae el mensaje legible que devuelve la API ({ message }) o uno por defecto. */
export function mensajeDeError(error: unknown, porDefecto: string): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) {
      return 'No se pudo contactar con el servidor.';
    }
    const cuerpo = error.error;
    if (cuerpo && typeof cuerpo === 'object' && typeof cuerpo.message === 'string') {
      return cuerpo.message;
    }
    if (typeof cuerpo === 'string' && cuerpo.trim()) {
      return cuerpo;
    }
  }
  return porDefecto;
}

/**
 * Igual que mensajeDeError, para peticiones con responseType 'blob':
 * en ese caso el cuerpo del error también llega como Blob y hay que leerlo.
 */
export async function mensajeDeErrorBlob(error: unknown, porDefecto: string): Promise<string> {
  if (error instanceof HttpErrorResponse && error.error instanceof Blob) {
    try {
      const cuerpo = JSON.parse(await error.error.text());
      if (typeof cuerpo?.message === 'string') {
        return cuerpo.message;
      }
    } catch {
      // El cuerpo no era JSON: se usa el mensaje por defecto.
    }
  }
  return mensajeDeError(error, porDefecto);
}

/** Descarga un archivo recibido como Blob con el nombre indicado. */
export function descargarArchivo(blob: Blob, nombre: string): void {
  const url = URL.createObjectURL(blob);
  const enlace = document.createElement('a');
  enlace.href = url;
  enlace.download = nombre;
  enlace.click();
  URL.revokeObjectURL(url);
}
