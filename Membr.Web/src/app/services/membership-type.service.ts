import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import type { components } from '@/api/schema';

export type MembershipTypeDto = components['schemas']['MembershipTypeDto'];
export type CreateMembershipTypeRequest = components['schemas']['CreateMembershipTypeRequest'];

@Injectable({ providedIn: 'root'})
export class MembershipTypeService {
  private http = inject(HttpClient);
  private base = '/admin/membershiptypes';

  list(): Observable<MembershipTypeDto[]>{
    return this.http.get<MembershipTypeDto[]>(`${this.base}/`);
  }

  create(request: CreateMembershipTypeRequest): Observable<MembershipTypeDto> {
    return this.http.post<MembershipTypeDto>(this.base, request);
  }
}