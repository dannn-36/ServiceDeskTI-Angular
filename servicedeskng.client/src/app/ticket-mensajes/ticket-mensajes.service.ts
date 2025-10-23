import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketMensajesComponent } from './ticket-mensajes.component';

describe('TicketMensajesComponent', () => {
  let component: TicketMensajesComponent;
  let fixture: ComponentFixture<TicketMensajesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TicketMensajesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TicketMensajesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
