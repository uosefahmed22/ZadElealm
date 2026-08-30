import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';

import { appEnvironment } from '../config/app-environment';
import { AuthRefreshService } from '../auth/auth-refresh.service';
import { AuthSessionService } from '../auth/auth-session.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(AuthSessionService);
  const refreshService = inject(AuthRefreshService);
  const token = session.accessToken();
  const isApiRequest = request.url.startsWith(`${appEnvironment.apiBaseUrl}/`);
  const isRefreshRequest = request.url.endsWith('/Account/refresh-token');

  if (!isApiRequest || !token) {
    return next(request);
  }

  const authenticatedRequest = request.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });

  return next(authenticatedRequest).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isRefreshRequest) {
        return throwError(() => error);
      }

      return refreshService.refresh().pipe(
        catchError((refreshError: unknown) => {
          session.clearSession();
          return throwError(() => refreshError);
        }),
        switchMap((user) =>
          next(
            request.clone({
              setHeaders: { Authorization: `Bearer ${user.token}` },
            }),
          ),
        ),
      );
    }),
  );
};
