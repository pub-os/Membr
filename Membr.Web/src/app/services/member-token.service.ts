import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '@/api/schema';

export type TokenDto = components['schemas']['TokenDto'];
export type TokenType = components['schemas']['TokenType'];
export type CreateMemberTokenRequest = components['schemas']['CreateMemberTokenRequest'];
export type TokenLookupDto = components['schemas']['TokenLookupDto'];

@Injectable({ providedIn: 'root' })
export class MemberTokenService {
  private http = inject(HttpClient);

  listForMember(memberId: number | string): Observable<TokenDto[]> {
    return this.http.get<TokenDto[]>(`/admin/members/${memberId}/tokens/`);
  }

  create(memberId: number | string, request: CreateMemberTokenRequest): Observable<TokenDto> {
    return this.http.post<TokenDto>(`/admin/members/${memberId}/tokens`, request);
  }

  revoke(memberId: number | string, tokenId: TokenDto['id']): Observable<void> {
    return this.http.delete<void>(`/admin/members/${memberId}/tokens/${tokenId}`);
  }

  lookup(value: string): Observable<TokenLookupDto> {
    return this.http.get<TokenLookupDto>('/admin/tokens/lookup', { params: { value } });
  }
}
