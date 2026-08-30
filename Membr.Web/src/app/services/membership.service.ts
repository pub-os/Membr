import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '@/api/schema';

export type MembershipDto = components['schemas']['MembershipDto'];
export type CreateMembershipRequest = components['schemas']['CreateMembershipRequest'];

@Injectable({ providedIn: 'root' })
export class MembershipService {
  private http = inject(HttpClient);

  list(memberId: MembershipDto['memberId']): Observable<MembershipDto[]> {
    return this.http.get<MembershipDto[]>(`/admin/members/${memberId}/memberships/`);
  }

  create(memberId: MembershipDto['memberId'], request: CreateMembershipRequest): Observable<MembershipDto> {
    return this.http.post<MembershipDto>(`/admin/members/${memberId}/memberships/`, request);
  }

  renew(memberId: MembershipDto['memberId'], membershipId: MembershipDto['id']): Observable<MembershipDto> {
    return this.http.post<MembershipDto>(`/admin/members/${memberId}/memberships/${membershipId}/renew`, {});
  }
}
