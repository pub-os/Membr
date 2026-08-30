import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, finalize, map, of, shareReplay, switchMap, tap } from 'rxjs';
import type { components } from '@/api/schema';

export type MeResponse = components['schemas']['MeResponse'];
type LoginRequest = components['schemas']['LoginRequest'];
type AccessTokenResponse = components['schemas']['AccessTokenResponse'];

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly accessTokenSignal = signal<string | null>(null);
  private readonly userSignal = signal<MeResponse | null>(null);

  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.userSignal() !== null);
  readonly isAdmin = computed(() => this.userSignal()?.roles.includes('Admin') ?? false);

  private refreshInFlight: Observable<string | null> | null = null;

  get accessToken(): string | null {
    return this.accessTokenSignal();
  }

  login(email: string, password: string): Observable<MeResponse> {
    const body: LoginRequest = { email, password };
    return this.http.post<AccessTokenResponse>('/auth/login', body, { withCredentials: true }).pipe(
      tap(res => this.accessTokenSignal.set(res.accessToken)),
      switchMap(() => this.loadCurrentUser()),
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>('/auth/logout', {}, { withCredentials: true }).pipe(
      catchError(() => of(void 0)),
      tap(() => this.clearSession()),
    );
  }

  /** Called once at app startup: tries to turn the httpOnly refresh cookie back into a session. */
  restoreSession(): Observable<MeResponse | null> {
    return this.refreshAccessToken().pipe(
      switchMap(token => (token ? this.loadCurrentUser() : of(null))),
      catchError(() => {
        this.clearSession();
        return of(null);
      }),
    );
  }

  /** Shared by any number of concurrent callers (app init, interceptor retries) — only ever one refresh in flight. */
  refreshAccessToken(): Observable<string | null> {
    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    this.refreshInFlight = this.http.post<AccessTokenResponse>('/auth/refresh', {}, { withCredentials: true }).pipe(
      map(res => res.accessToken),
      tap(token => this.accessTokenSignal.set(token)),
      catchError(() => {
        this.clearSession();
        return of(null);
      }),
      finalize(() => {
        this.refreshInFlight = null;
      }),
      shareReplay(1),
    );

    return this.refreshInFlight;
  }

  private loadCurrentUser(): Observable<MeResponse> {
    return this.http.get<MeResponse>('/auth/me').pipe(tap(user => this.userSignal.set(user)));
  }

  private clearSession(): void {
    this.accessTokenSignal.set(null);
    this.userSignal.set(null);
  }
}