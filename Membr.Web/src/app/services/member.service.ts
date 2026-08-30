import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '@/api/schema';

export type MemberDto = components['schemas']['MemberDto'];
export type CreateMemberRequest = components['schemas']['CreateMemberRequest'];
export type PagedResultOfMemberDto = components['schemas']['PagedResultOfMemberDto'];

@Injectable({ providedIn: 'root' })
export class MemberService {
  private http = inject(HttpClient);
  private base = '/admin/members';

  search(query: string, page = 1, pageSize = 25): Observable<PagedResultOfMemberDto> {
    return this.http.get<PagedResultOfMemberDto>(`${this.base}/search`, {
      params: { q: query, page, pageSize }
    });
  }

  get(id: MemberDto['id']): Observable<MemberDto> {
    return this.http.get<MemberDto>(`${this.base}/${id}`);
  }

  create(request: CreateMemberRequest): Observable<MemberDto> {
    return this.http.post<MemberDto>(this.base, request);
  }

  list(page = 1, pageSize = 25): Observable<PagedResultOfMemberDto> {
    return this.http.get<PagedResultOfMemberDto>(`${this.base}/`, {
      params: { page, pageSize }
    });
  }

}