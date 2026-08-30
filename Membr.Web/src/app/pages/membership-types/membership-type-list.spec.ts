import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MembershipTypeListComponent } from './membership-type-list';

describe('MembershipTypeListComponent', () => {
  let component: MembershipTypeListComponent;
  let fixture: ComponentFixture<MembershipTypeListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MembershipTypeListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MembershipTypeListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});