import { Component, OnInit, inject } from '@angular/core';
import { RegistroAuditoria, mensajeDeError } from '../core/modelos';
import { AuditoriaService } from './auditoria.service';

interface OpcionAccion {
  valor: string;
  etiqueta: string;
}

/**
 * Bitácora de seguridad: quién hizo qué y cuándo
 * (inicios de sesión, intentos fallidos, altas y bajas, respaldos...).
 */
@Component({
  selector: 'app-auditoria',
  templateUrl: './auditoria.component.html',
  styleUrls: ['./auditoria.component.css']
})
export class AuditoriaComponent implements OnInit {
  private readonly auditoria = inject(AuditoriaService);

  readonly acciones: OpcionAccion[] = [
    { valor: '', etiqueta: 'Todas las acciones' },
    { valor: 'LOGIN', etiqueta: 'Inicios de sesión' },
    { valor: 'LOGIN_FALLIDO', etiqueta: 'Intentos fallidos' },
    { valor: 'LOGOUT', etiqueta: 'Cierres de sesión' },
    { valor: 'USUARIO_CREADO', etiqueta: 'Usuarios creados' },
    { valor: 'USUARIO_ACTUALIZADO', etiqueta: 'Usuarios modificados' },
    { valor: 'USUARIO_ELIMINADO', etiqueta: 'Usuarios dados de baja' },
    { valor: 'TICKET_CREADO', etiqueta: 'Tickets creados' },
    { valor: 'TICKET_ACTUALIZADO', etiqueta: 'Tickets modificados' },
    { valor: 'TICKET_ASIGNADO', etiqueta: 'Asignaciones' },
    { valor: 'TICKET_ESCALADO', etiqueta: 'Escalados' },
    { valor: 'TICKETS_REDISTRIBUIDOS', etiqueta: 'Redistribuciones' },
    { valor: 'RESPALDO_DESCARGADO', etiqueta: 'Respaldos descargados' },
    { valor: 'RESPALDO_RESTAURADO', etiqueta: 'Restauraciones' }
  ];

  registros: RegistroAuditoria[] = [];
  accionSeleccionada = '';
  cargando = false;
  error = '';

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.cargando = true;
    this.error = '';

    this.auditoria.listar(200, this.accionSeleccionada).subscribe({
      next: registros => {
        this.registros = registros;
        this.cargando = false;
      },
      error: err => {
        this.error = mensajeDeError(err, 'No se pudo cargar la bitácora de auditoría.');
        this.cargando = false;
      }
    });
  }

  /** Intentos de inicio de sesión fallidos en las últimas 24 horas (de los registros cargados). */
  get fallidosUltimoDia(): number {
    const limite = Date.now() - 24 * 60 * 60 * 1000;
    return this.registros.filter(r =>
      r.accionAuditoria === 'LOGIN_FALLIDO'
      && r.fechaAuditoria
      && new Date(r.fechaAuditoria).getTime() >= limite).length;
  }

  etiquetaAccion(accion: string): string {
    return this.acciones.find(a => a.valor === accion)?.etiqueta ?? accion;
  }

  claseAccion(accion: string): string {
    if (accion === 'LOGIN_FALLIDO') {
      return 'bg-red-100 text-red-800';
    }
    if (accion.endsWith('ELIMINADO') || accion.startsWith('RESPALDO')) {
      return 'bg-orange-100 text-orange-800';
    }
    if (accion.startsWith('LOGIN') || accion === 'LOGOUT') {
      return 'bg-blue-100 text-blue-800';
    }
    return 'bg-gray-100 text-gray-800';
  }

  trackPorId(_: number, registro: RegistroAuditoria): number {
    return registro.idAuditoria;
  }
}
