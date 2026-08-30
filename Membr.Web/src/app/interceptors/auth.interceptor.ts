import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '@/services/auth.service';

const AUTH_EXEMPT_PATHS = ['/auth/login', '/auth/refresh'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const isExempt = AUTH_EXEMPT_PATHS.some(path => req.url.startsWith(path));
  const token = auth.accessToken;

  const authedReq = !isExempt && token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authedReq).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isExempt) {
        return throwError(() => error);
      }

      return auth.refreshAccessToken().pipe(
        switchMap(newToken => {
          if (!newToken) {
            router.navigate(['/login']);
            return throwError(() => error);
          }

          const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
          return next(retryReq);
        }),
      );
    }),
  );
};