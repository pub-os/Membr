import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import type { components } from '@/api/schema';

export type UserDto = components['schemas']['UserDto'];
export type CreateUserRequest = components['schemas']['CreateUserRequest'];

@Injectable({ providedIn: 'root'})
export class UserService {
  private http = inject(HttpClient);
  private base = '/admin/users';

  list(): Observable<UserDto[]>{
    return this.http.get<UserDto[]>(`${this.base}/`);
  }

  create(request: CreateUserRequest): Observable<UserDto> {
    return this.http.post<UserDto>(this.base, request);
  }

  listRoles(): Observable<string[]> {
    return this.http.get<string[]>(`${this.base}/roles`);
  }
}
