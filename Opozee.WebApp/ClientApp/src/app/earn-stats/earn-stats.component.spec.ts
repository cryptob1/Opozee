import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EarnStatsComponent } from './earn-stats.component';

describe('EarnStatsComponent', () => {
  let component: EarnStatsComponent;
  let fixture: ComponentFixture<EarnStatsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EarnStatsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EarnStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
