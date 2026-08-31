import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '@/api/schema';

export type ContactDto = components['schemas']['ContactDto'];
export type CreateMemberContactRequest = components['schemas']['CreateMemberContactRequest'];
export type UpdateMemberContactRequest = components['schemas']['UpdateMemberContactRequest'];

@Injectable({ providedIn: 'root' })
export class MemberContactService {
  private http = inject(HttpClient);

  listForMember(memberId: number | string): Observable<ContactDto[]> {
    return this.http.get<ContactDto[]>(`/admin/members/${memberId}/contacts/`);
  }

  create(memberId: number | string, request: CreateMemberContactRequest): Observable<ContactDto> {
    return this.http.post<ContactDto>(`/admin/members/${memberId}/contacts`, request);
  }

  update(memberId: number | string, contactId: ContactDto['id'], request: UpdateMemberContactRequest): Observable<ContactDto> {
    return this.http.put<ContactDto>(`/admin/members/${memberId}/contacts/${contactId}`, request);
  }

  delete(memberId: number | string, contactId: ContactDto['id']): Observable<void> {
    return this.http.delete<void>(`/admin/members/${memberId}/contacts/${contactId}`);
  }
}
