import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '@/api/schema';

export type UdfDefinitionDto = components['schemas']['UdfDefinitionDto'];
export type UdfFieldType = components['schemas']['UdfFieldType'];
export type CreateUdfDefinitionRequest = components['schemas']['CreateUdfDefinitionRequest'];
export type UpdateUdfDefinitionRequest = components['schemas']['UpdateUdfDefinitionRequest'];

@Injectable({ providedIn: 'root' })
export class UdfFieldService {
  private http = inject(HttpClient);
  private base = '/admin/udffields';

  list(): Observable<UdfDefinitionDto[]> {
    return this.http.get<UdfDefinitionDto[]>(`${this.base}/`);
  }

  get(id: UdfDefinitionDto['id']): Observable<UdfDefinitionDto> {
    return this.http.get<UdfDefinitionDto>(`${this.base}/${id}`);
  }

  create(request: CreateUdfDefinitionRequest): Observable<UdfDefinitionDto> {
    return this.http.post<UdfDefinitionDto>(this.base, request);
  }

  update(id: UdfDefinitionDto['id'], request: UpdateUdfDefinitionRequest): Observable<UdfDefinitionDto> {
    return this.http.put<UdfDefinitionDto>(`${this.base}/${id}`, request);
  }

  delete(id: UdfDefinitionDto['id']): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  applyDefault(id: UdfDefinitionDto['id']): Observable<UdfDefinitionDto> {
    return this.http.post<UdfDefinitionDto>(`${this.base}/${id}/apply-default`, {});
  }
}
