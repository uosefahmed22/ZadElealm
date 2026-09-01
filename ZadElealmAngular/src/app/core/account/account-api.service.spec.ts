import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { appEnvironment } from '../config/app-environment';
import { AccountApiService } from './account-api.service';

describe('AccountApiService', () => {
  let service: AccountApiService;
  let httpMock: HttpTestingController;
  const accountUrl = `${appEnvironment.apiBaseUrl}/Account`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AccountApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('loads and updates the profile through the real account routes', () => {
    service.getProfile().subscribe();
    const getRequest = httpMock.expectOne(`${accountUrl}/get-User-Profile`);
    expect(getRequest.request.method).toBe('GET');
    getRequest.flush({ statusCode: 200, data: profile() });

    service.updateProfile({ displayName: 'يوسف أحمد', phoneNumber: '+201001234567' }).subscribe();
    const updateRequest = httpMock.expectOne(`${accountUrl}/update-profile`);
    expect(updateRequest.request.method).toBe('POST');
    expect(updateRequest.request.body).toEqual({
      displayName: 'يوسف أحمد',
      phoneNumber: '+201001234567',
    });
    updateRequest.flush({ statusCode: 200 });
  });

  it('uploads the selected image using the backend file field', () => {
    const file = new File(['image'], 'avatar.png', { type: 'image/png' });
    service.updateProfileImage(file).subscribe();

    const request = httpMock.expectOne(`${accountUrl}/update-profile-image`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeInstanceOf(FormData);
    expect((request.request.body as FormData).get('file')).toBe(file);
    request.flush({ statusCode: 200 });
  });

  it('uses the two-step email change contract', () => {
    service.sendEmailOtp({ newEmail: 'new@test.com', password: 'Password123!' }).subscribe();
    const otpRequest = httpMock.expectOne(`${accountUrl}/send-email-otp`);
    expect(otpRequest.request.body).toEqual({
      newEmail: 'new@test.com',
      password: 'Password123!',
    });
    otpRequest.flush({ statusCode: 200 });

    service.updateEmail({ newEmail: 'new@test.com', token: '123456' }).subscribe();
    const updateRequest = httpMock.expectOne(`${accountUrl}/update-email`);
    expect(updateRequest.request.body).toEqual({ newEmail: 'new@test.com', token: '123456' });
    updateRequest.flush({ statusCode: 200 });
  });

  it('sends the password only in the protected account deletion body', () => {
    service.deleteAccount('Password123!').subscribe();

    const request = httpMock.expectOne(`${accountUrl}/delete-account`);
    expect(request.request.method).toBe('DELETE');
    expect(request.request.body).toEqual({ password: 'Password123!' });
    request.flush({ statusCode: 200, message: 'تم حذف الحساب بنجاح' });
  });
});

function profile() {
  return {
    id: '1',
    displayName: 'يوسف أحمد',
    email: 'user@test.com',
    imageUrl: null,
    userName: 'user@test.com',
    phoneNumber: null,
  };
}
