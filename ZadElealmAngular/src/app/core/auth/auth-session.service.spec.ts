import { TestBed } from '@angular/core/testing';

import { AuthSessionService } from './auth-session.service';

describe('AuthSessionService', () => {
  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({});
  });

  afterEach(() => sessionStorage.clear());

  it('stores and exposes the complete token pair', () => {
    const service = TestBed.inject(AuthSessionService);

    service.setSession({
      displayName: 'طالب',
      email: 'user@test.com',
      token: 'access',
      refreshToken: 'refresh',
    });

    expect(service.isAuthenticated()).toBe(true);
    expect(service.getTokenRequest()).toEqual({ token: 'access', refreshToken: 'refresh' });
  });

  it('discards an incomplete stored session', () => {
    sessionStorage.setItem('zad-elealm.auth.session', JSON.stringify({ token: 'access' }));

    const service = TestBed.inject(AuthSessionService);

    expect(service.isAuthenticated()).toBe(false);
    expect(sessionStorage.getItem('zad-elealm.auth.session')).toBeNull();
  });

  it('updates visible identity without losing either token', () => {
    const service = TestBed.inject(AuthSessionService);
    service.setSession({
      displayName: 'طالب',
      email: 'user@test.com',
      token: 'access',
      refreshToken: 'refresh',
    });

    service.updateIdentity('طالب جديد', 'new@test.com');

    expect(service.user()).toEqual({
      displayName: 'طالب جديد',
      email: 'new@test.com',
      token: 'access',
      refreshToken: 'refresh',
    });
  });
});
