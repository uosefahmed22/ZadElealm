import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';

import { appEnvironment } from '../config/app-environment';
import { AuthApiService } from './auth-api.service';

describe('AuthApiService', () => {
  let service: AuthApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(AuthApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('posts login payload to the real account route', () => {
    service.login({ email: 'user@test.com', password: '12345678' }).subscribe();

    const request = httpMock.expectOne(`${appEnvironment.apiBaseUrl}/Account/login`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'user@test.com',
      password: '12345678',
    });

    request.flush({
      statusCode: 200,
      data: {
        displayName: 'طالب',
        email: 'user@test.com',
        token: 'access',
        refreshToken: 'refresh',
      },
    });
  });

  it('sends forgot password email as query params because backend expects a simple parameter', () => {
    service.forgotPassword('user@test.com').subscribe();

    const request = httpMock.expectOne(
      (candidate) =>
        candidate.url === `${appEnvironment.apiBaseUrl}/Account/forget-password` &&
        candidate.params.get('email') === 'user@test.com',
    );

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush({ statusCode: 200, message: 'ok' });
  });

  it('gets the current user from the protected account route', () => {
    service.currentUser().subscribe();

    const request = httpMock.expectOne(`${appEnvironment.apiBaseUrl}/Account/current-user`);
    expect(request.request.method).toBe('GET');
    request.flush({
      statusCode: 200,
      data: {
        displayName: 'طالب',
        email: 'user@test.com',
        token: 'access',
        refreshToken: 'refresh',
      },
    });
  });
});
