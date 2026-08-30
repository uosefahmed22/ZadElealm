import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { AuthSessionService } from '../auth/auth-session.service';
import { appEnvironment } from '../config/app-environment';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let session: AuthSessionService;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    session = TestBed.inject(AuthSessionService);
    session.setSession({
      displayName: 'طالب',
      email: 'user@test.com',
      token: 'expired-access',
      refreshToken: 'valid-refresh',
    });
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('does not leak the access token to non-api requests', () => {
    http.get('https://example.com/image').subscribe();

    const request = httpMock.expectOne('https://example.com/image');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });

  it('refreshes once and retries an api request with the new access token', () => {
    const protectedUrl = `${appEnvironment.apiBaseUrl}/Account/current-user`;
    http.get(protectedUrl).subscribe();

    const original = httpMock.expectOne(protectedUrl);
    expect(original.request.headers.get('Authorization')).toBe('Bearer expired-access');
    original.flush({}, { status: 401, statusText: 'Unauthorized' });

    const refresh = httpMock.expectOne(`${appEnvironment.apiBaseUrl}/Account/refresh-token`);
    expect(refresh.request.body).toEqual({
      token: 'expired-access',
      refreshToken: 'valid-refresh',
    });
    refresh.flush({
      statusCode: 200,
      data: {
        displayName: 'طالب',
        email: 'user@test.com',
        token: 'new-access',
        refreshToken: 'new-refresh',
      },
    });

    const retried = httpMock.expectOne(protectedUrl);
    expect(retried.request.headers.get('Authorization')).toBe('Bearer new-access');
    retried.flush({ statusCode: 200 });
    expect(session.accessToken()).toBe('new-access');
  });

  it('clears the session when refreshing fails', () => {
    const protectedUrl = `${appEnvironment.apiBaseUrl}/Account/current-user`;
    http.get(protectedUrl).subscribe({ error: () => undefined });

    httpMock.expectOne(protectedUrl).flush({}, { status: 401, statusText: 'Unauthorized' });
    httpMock
      .expectOne(`${appEnvironment.apiBaseUrl}/Account/refresh-token`)
      .flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(session.isAuthenticated()).toBe(false);
  });

  it('keeps the refreshed session when the retried request is forbidden', () => {
    const protectedUrl = `${appEnvironment.apiBaseUrl}/admin-only`;
    http.get(protectedUrl).subscribe({ error: () => undefined });

    httpMock.expectOne(protectedUrl).flush({}, { status: 401, statusText: 'Unauthorized' });
    httpMock.expectOne(`${appEnvironment.apiBaseUrl}/Account/refresh-token`).flush({
      statusCode: 200,
      data: {
        displayName: 'طالب',
        email: 'user@test.com',
        token: 'new-access',
        refreshToken: 'new-refresh',
      },
    });
    httpMock.expectOne(protectedUrl).flush({}, { status: 403, statusText: 'Forbidden' });

    expect(session.accessToken()).toBe('new-access');
  });
});
