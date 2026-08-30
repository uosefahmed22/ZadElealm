import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';

import { AuthSessionService } from '../auth/auth-session.service';
import { authGuard } from './auth.guard';
import { guestGuard } from './guest.guard';

describe('auth guards', () => {
  let session: AuthSessionService;
  let router: Router;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    session = TestBed.inject(AuthSessionService);
    router = TestBed.inject(Router);
  });

  afterEach(() => sessionStorage.clear());

  it('redirects a guest to login and preserves the protected return url', () => {
    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/app' } as RouterStateSnapshot),
    );

    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe('/login?returnUrl=%2Fapp');
  });

  it('allows an authenticated user into the student area', () => {
    session.setSession({
      displayName: 'طالب',
      email: 'user@test.com',
      token: 'access',
      refreshToken: 'refresh',
    });

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/app' } as RouterStateSnapshot),
    );

    expect(result).toBe(true);
  });

  it('keeps authenticated users out of guest auth pages', () => {
    session.setSession({
      displayName: 'طالب',
      email: 'user@test.com',
      token: 'access',
      refreshToken: 'refresh',
    });

    const result = TestBed.runInInjectionContext(() =>
      guestGuard({} as ActivatedRouteSnapshot, { url: '/login' } as RouterStateSnapshot),
    );

    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe('/app');
  });
});
