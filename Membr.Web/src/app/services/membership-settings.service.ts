import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '@/api/schema';

export type MembershipSettingsDto = components['schemas']['MembershipSettingsDto'];
export type UpdateMembershipSettingsRequest = components['schemas']['UpdateMembershipSettingsRequest'];

@Injectable({ providedIn: 'root' })
export class MembershipSettingsService {
  private http = inject(HttpClient);
  private base = '/admin/settings/membership';

  get(): Observable<MembershipSettingsDto> {
    return this.http.get<MembershipSettingsDto>(`${this.base}/`);
  }

  update(request: UpdateMembershipSettingsRequest): Observable<MembershipSettingsDto> {
    return this.http.put<MembershipSettingsDto>(`${this.base}/`, request);
  }
}
