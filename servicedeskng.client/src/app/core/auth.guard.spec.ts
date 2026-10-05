import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Route, Router, UrlSegment, UrlTree, provideRouter } from '@angular/router';
import { AuthService } from './auth.service';
import { invitadoGuard, rolGuard } from './auth.guard';
import { Rol } from './modelos';

describe('Guards de autenticación', () => {
  let auth: AuthService;
  let http: HttpTestingController;
  let router: Router;

  const iniciarSesionComo = (rol: Rol) => {
    auth.login('usuario@empresa.com', 'clave').subscribe();
    http.expectOne('/api/auth/login').flush({
      idUsuario: 1, nombreUsuario: 'Prueba', correoUsuario: 'usuario@empresa.com', rol
    });
  };

  const evaluarRol = (...roles: Rol[]) =>
    TestBed.runInInjectionContext(() => rolGuard(...roles)({} as Route, [] as UrlSegment[]));

  const evaluarInvitado = () =>
    TestBed.runInInjectionContext(() => invitadoGuard({} as never, {} as never));

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])]
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  it('sin sesión, cualquier panel redirige al login', () => {
    const resultado = evaluarRol('Administrador') as UrlTree;
    expect(router.serializeUrl(resultado)).toBe('/hogar');
  });

  it('con el rol correcto, deja pasar', () => {
    iniciarSesionComo('Administrador');
    expect(evaluarRol('Administrador')).toBeTrue();
  });

  it('con otro rol, lleva a su propio panel en vez de al pedido', () => {
    iniciarSesionComo('Cliente');
    const resultado = evaluarRol('Administrador') as UrlTree;
    expect(router.serializeUrl(resultado)).toBe('/end-user');
  });

  it('la pantalla de login redirige al panel si ya hay sesión', () => {
    expect(evaluarInvitado()).toBeTrue();

    iniciarSesionComo('Agente');
    expect(router.serializeUrl(evaluarInvitado() as UrlTree)).toBe('/agente');
  });
});
