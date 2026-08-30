import { Injectable, inject } from '@angular/core';
import { Observable, catchError, finalize, map, of } from 'rxjs';

import { AuthApiService } from './auth-api.service';
import { AuthSessionService } from './auth-session.service';

@Injectable({ providedIn: 'root' })
export class AuthFacadeService {
  private readonly authApi = inject(AuthApiService);
  private readonly session = inject(AuthSessionService);

  logout(): Observable<void> {
    const tokenRequest = this.session.getTokenRequest();
    if (!tokenRequest) {
      this.session.clearSession();
      return of(undefined);
    }

    return this.authApi.revokeToken(tokenRequest).pipe(
      map(() => undefined),
      catchError(() => of(undefined)),
      finalize(() => this.session.clearSession()),
    );
  }
}
