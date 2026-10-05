import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let auth: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
    spyOn(auth, 'terminarSesionLocal');
  });

  afterEach(() => backend.verify());

  it('un 401 en una petición normal cierra la sesión local', () => {
    http.get('/api/tickets').subscribe({ error: () => undefined });
    backend.expectOne('/api/tickets').flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(auth.terminarSesionLocal).toHaveBeenCalled();
  });

  it('un 401 del login (credenciales erróneas) no provoca redirección', () => {
    http.post('/api/auth/login', {}).subscribe({ error: () => undefined });
    backend.expectOne('/api/auth/login').flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(auth.terminarSesionLocal).not.toHaveBeenCalled();
  });

  it('un 403 no cierra la sesión: el usuario sigue autenticado', () => {
    http.get('/api/backup').subscribe({ error: () => undefined });
    backend.expectOne('/api/backup').flush({}, { status: 403, statusText: 'Forbidden' });

    expect(auth.terminarSesionLocal).not.toHaveBeenCalled();
  });
});
