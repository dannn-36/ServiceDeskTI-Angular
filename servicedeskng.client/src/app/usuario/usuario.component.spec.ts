import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { UsuarioComponent } from './usuario.component';

// UsuarioComponent ya no está enrutado (su función la cubre el panel de administración).
// Se mantiene la prueba mientras el archivo exista en el repositorio.
describe('UsuarioComponent', () => {
  let fixture: ComponentFixture<UsuarioComponent>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [UsuarioComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(UsuarioComponent);
    http = TestBed.inject(HttpTestingController);
  });

  it('carga la lista de usuarios al iniciarse', () => {
    fixture.detectChanges();

    http.expectOne('/api/Usuario').flush([]);

    expect(fixture.componentInstance.usuarios).toEqual([]);
    expect(fixture.componentInstance.loading).toBeFalse();
  });
});
