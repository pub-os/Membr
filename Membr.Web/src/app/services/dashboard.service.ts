import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import type { components } from '@/api/schema';

export type DashboardStatsDto = components['schemas']['DashboardStatsDto'];

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private http = inject(HttpClient);

  getStats(): Observable<DashboardStatsDto> {
    return this.http.get<DashboardStatsDto>('/admin/dashboard/stats');
  }
}
