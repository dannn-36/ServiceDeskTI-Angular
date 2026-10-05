import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthService } from './auth.service';
import { SesionUsuario } from './modelos';

const AGENTE: SesionUsuario = {
  idUsuario: 3,
  nombreUsuario: 'Alberto Agente',
  correoUsuario: 'agente@empresa.com',
  rol: 'Agente',
  idAgente: 7
};

describe('AuthService', () => {
  let auth: AuthService;
  let http: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])]
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);
  });

  afterEach(() => http.verify());

  it('guarda la identidad devuelta por el login, solo en memoria', () => {
    spyOn(localStorage, 'setItem');

    auth.login('agente@empresa.com', 'clave').subscribe();
    const peticion = http.expectOne('/api/auth/login');
    expect(peticion.request.body).toEqual({ correoUsuario: 'agente@empresa.com', contrasenaUsuario: 'clave' });
    peticion.flush(AGENTE);

    expect(auth.usuario()).toEqual(AGENTE);
    expect(auth.autenticado()).toBeTrue();
    expect(localStorage.setItem).not.toHaveBeenCalled();
  });

  it('restaura la sesión desde la cookie con /api/auth/me', async () => {
    const restauracion = auth.restaurarSesion();
    http.expectOne('/api/auth/me').flush(AGENTE);
    await restauracion;

    expect(auth.usuario()?.idAgente).toBe(7);
  });

  it('queda sin sesión si /api/auth/me responde 401', async () => {
    const restauracion = auth.restaurarSesion();
    http.expectOne('/api/auth/me').flush({ message: 'Debe iniciar sesión.' }, { status: 401, statusText: 'Unauthorized' });
    await restauracion;

    expect(auth.usuario()).toBeNull();
  });

  it('al cerrar sesión avisa al servidor, olvida la identidad y vuelve al login', () => {
    auth.login('agente@empresa.com', 'clave').subscribe();
    http.expectOne('/api/auth/login').flush(AGENTE);

    auth.logout();
    http.expectOne('/api/auth/logout').flush(null, { status: 204, statusText: 'No Content' });

    expect(auth.usuario()).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/hogar']);
  });

  it('calcula la pantalla de inicio según el rol', () => {
    expect(auth.rutaInicio()).toBe('/hogar');

    auth.login('x', 'y').subscribe();
    http.expectOne('/api/auth/login').flush({ ...AGENTE, rol: 'Supervisor' });

    expect(auth.rutaInicio()).toBe('/supervisor');
  });

  it('refleja localmente un cambio de perfil', () => {
    auth.login('x', 'y').subscribe();
    http.expectOne('/api/auth/login').flush(AGENTE);

    auth.actualizarDatosLocales('Nuevo Nombre', 'nuevo@empresa.com');

    expect(auth.usuario()?.nombreUsuario).toBe('Nuevo Nombre');
    expect(auth.usuario()?.rol).toBe('Agente');
  });
});
