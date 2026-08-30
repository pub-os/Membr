import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';

import { MemberOverviewComponent } from './member-overview';

describe('MemberOverviewComponent', () => {
  let component: MemberOverviewComponent;
  let fixture: ComponentFixture<MemberOverviewComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberOverviewComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: '1' }) } },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MemberOverviewComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();

    httpMock.expectOne('/admin/members/1').flush({ id: 1, firstName: 'Ada', surname: 'Lovelace', dateOfBirth: '1990-01-01' });
    httpMock.expectOne('/admin/members/1/memberships/').flush([
      { id: 1, memberId: 1, membershipTypeId: 1, membershipTypeName: 'Standard', startDate: '2026-01-01T00:00:00Z', endDate: '2027-01-01T00:00:00Z', isActive: true },
      { id: 2, memberId: 1, membershipTypeId: 2, membershipTypeName: 'Premium', startDate: '2026-01-01T00:00:00Z', endDate: '2027-01-01T00:00:00Z', isActive: true },
    ]);
    httpMock.expectOne('/admin/membershiptypes/').flush([]);
    await fixture.whenStable();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('flags multiple active memberships', () => {
    expect(component.hasMultipleActiveMemberships()).toBe(true);
    expect(component.activeMembershipCount()).toBe(2);
  });
});
