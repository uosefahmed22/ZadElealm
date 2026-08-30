import { Injectable, inject } from '@angular/core';
import { Observable, finalize, map, shareReplay, throwError } from 'rxjs';

import { AuthApiService } from './auth-api.service';
import { AuthSessionService } from './auth-session.service';
import { UserDto } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthRefreshService {
  private readonly authApi = inject(AuthApiService);
  private readonly session = inject(AuthSessionService);
  private refreshRequest$: Observable<UserDto> | null = null;

  refresh(): Observable<UserDto> {
    const tokenRequest = this.session.getTokenRequest();
    if (!tokenRequest) {
      return throwError(() => new Error('No refresh token is available.'));
    }

    this.refreshRequest$ ??= this.authApi.refreshToken(tokenRequest).pipe(
      map((response) => response.data),
      map((user) => {
        this.session.setSession(user);
        return user;
      }),
      finalize(() => (this.refreshRequest$ = null)),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    return this.refreshRequest$;
  }
}
