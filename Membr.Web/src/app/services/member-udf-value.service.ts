import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '@/api/schema';

export type MemberUdfFieldDto = components['schemas']['MemberUdfFieldDto'];
export type MemberUdfValueDto = components['schemas']['MemberUdfValueDto'];
export type UdfValuesGridDto = components['schemas']['UdfValuesGridDto'];

@Injectable({ providedIn: 'root' })
export class MemberUdfValueService {
  private http = inject(HttpClient);

  listForMember(memberId: number | string): Observable<MemberUdfFieldDto[]> {
    return this.http.get<MemberUdfFieldDto[]>(`/admin/members/${memberId}/udf-values/`);
  }

  updateForMember(
    memberId: number | string,
    definitionId: MemberUdfFieldDto['definitionId'],
    value: string | null,
  ): Observable<MemberUdfValueDto> {
    return this.http.put<MemberUdfValueDto>(`/admin/members/${memberId}/udf-values/${definitionId}`, { value });
  }

  listGrid(): Observable<UdfValuesGridDto> {
    return this.http.get<UdfValuesGridDto>('/admin/udffields/values');
  }
}
