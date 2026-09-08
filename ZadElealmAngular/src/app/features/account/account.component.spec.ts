import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

import { AccountApiService } from '../../core/account/account-api.service';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import { AccountComponent } from './account.component';

describe('AccountComponent', () => {
  let accountApi: {
    getProfile: ReturnType<typeof vi.fn>;
    updateProfile: ReturnType<typeof vi.fn>;
    changePassword: ReturnType<typeof vi.fn>;
    updateProfileImage: ReturnType<typeof vi.fn>;
    removeProfileImage: ReturnType<typeof vi.fn>;
    sendEmailOtp: ReturnType<typeof vi.fn>;
    updateEmail: ReturnType<typeof vi.fn>;
    deleteAccount: ReturnType<typeof vi.fn>;
  };
  let session: {
    updateIdentity: ReturnType<typeof vi.fn>;
    clearSession: ReturnType<typeof vi.fn>;
    user: ReturnType<typeof vi.fn>;
  };
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    accountApi = {
      getProfile: vi.fn(() => of({ statusCode: 200, data: profile() })),
      updateProfile: vi.fn(() => of({ statusCode: 200, message: 'saved' })),
      changePassword: vi.fn(() => of({ statusCode: 200, message: 'changed' })),
      updateProfileImage: vi.fn(() => of({ statusCode: 200 })),
      removeProfileImage: vi.fn(() => of({ statusCode: 200 })),
      sendEmailOtp: vi.fn(() => of({ statusCode: 200, message: 'sent' })),
      updateEmail: vi.fn(() => of({ statusCode: 200 })),
      deleteAccount: vi.fn(() => of({ statusCode: 200, message: 'deleted' })),
    };
    session = { updateIdentity: vi.fn(), clearSession: vi.fn(), user: vi.fn(() => null) };
    router = { navigate: vi.fn(() => Promise.resolve(true)) };

    await TestBed.configureTestingModule({
      imports: [AccountComponent],
      providers: [
        { provide: AccountApiService, useValue: accountApi },
        { provide: AuthSessionService, useValue: session },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();
  });

  it('loads the real profile contract into the page and forms', () => {
    const fixture = TestBed.createComponent(AccountComponent);
    fixture.detectChanges();

    expect(accountApi.getProfile).toHaveBeenCalledOnce();
    expect(fixture.componentInstance.profileForm.getRawValue()).toEqual({
      displayName: 'يوسف أحمد',
      phoneNumber: '+201001234567',
    });
    expect(fixture.componentInstance.emailForm.controls.newEmail.value).toBe('');
    expect(fixture.nativeElement.textContent).toContain('يوسف أحمد');
  });

  it('saves profile data and synchronizes the visible session identity', () => {
    const fixture = TestBed.createComponent(AccountComponent);
    fixture.detectChanges();
    fixture.componentInstance.profileForm.setValue({
      displayName: 'يوسف محمد',
      phoneNumber: '+201009876543',
    });

    fixture.componentInstance.saveProfile();

    expect(accountApi.updateProfile).toHaveBeenCalledWith({
      displayName: 'يوسف محمد',
      phoneNumber: '+201009876543',
    });
    expect(session.updateIdentity).toHaveBeenCalledWith('يوسف محمد', 'user@test.com');
  });

  it('blocks mismatched passwords and submits a matching password', () => {
    const fixture = TestBed.createComponent(AccountComponent);
    fixture.detectChanges();
    fixture.componentInstance.passwordForm.setValue({
      currentPassword: 'OldPassword1!',
      newPassword: 'NewPassword1!',
      confirmPassword: 'different',
    });
    fixture.componentInstance.changePassword();
    expect(accountApi.changePassword).not.toHaveBeenCalled();

    fixture.componentInstance.passwordForm.controls.confirmPassword.setValue('NewPassword1!');
    fixture.componentInstance.changePassword();
    expect(accountApi.changePassword).toHaveBeenCalledWith({
      currentPassword: 'OldPassword1!',
      newPassword: 'NewPassword1!',
    });
  });

  it('completes email OTP flow then clears the stale JWT session', () => {
    const fixture = TestBed.createComponent(AccountComponent);
    fixture.detectChanges();
    fixture.componentInstance.emailForm.setValue({
      newEmail: 'new@test.com',
      password: 'Password123!',
      token: '',
    });

    fixture.componentInstance.sendEmailOtp();
    expect(accountApi.sendEmailOtp).toHaveBeenCalledWith({
      newEmail: 'new@test.com',
      password: 'Password123!',
    });

    fixture.componentInstance.emailForm.controls.token.setValue('123456');
    fixture.componentInstance.confirmEmail();
    expect(accountApi.updateEmail).toHaveBeenCalledWith({
      newEmail: 'new@test.com',
      token: '123456',
    });
    expect(session.clearSession).toHaveBeenCalledOnce();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { emailChanged: 'true' },
    });
  });

  it('rejects the current email locally and shows the error inside the email section', () => {
    const fixture = TestBed.createComponent(AccountComponent);
    fixture.detectChanges();
    fixture.componentInstance.emailForm.patchValue({
      newEmail: 'USER@test.com',
      password: 'Password123!',
    });

    fixture.componentInstance.sendEmailOtp();
    fixture.detectChanges();

    expect(accountApi.sendEmailOtp).not.toHaveBeenCalled();
    expect(fixture.componentInstance.errorAction()).toBe('email');
    expect(fixture.nativeElement.textContent).toContain('البريد الجديد هو نفس بريدك الحالي');
  });

  it('shows the API email error next to the email form', () => {
    accountApi.sendEmailOtp.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 400,
            error: { statusCode: 400, message: 'كلمة المرور غير صحيحة' },
          }),
      ),
    );
    const fixture = TestBed.createComponent(AccountComponent);
    fixture.detectChanges();
    fixture.componentInstance.emailForm.patchValue({
      newEmail: 'new@test.com',
      password: 'WrongPassword',
    });

    fixture.componentInstance.sendEmailOtp();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorAction()).toBe('email');
    expect(fixture.nativeElement.textContent).toContain('كلمة المرور غير صحيحة');
  });

  it('rejects unsupported profile images before calling the API', () => {
    const fixture = TestBed.createComponent(AccountComponent);
    fixture.detectChanges();
    const input = document.createElement('input');
    const file = new File(['text'], 'avatar.txt', { type: 'text/plain' });
    Object.defineProperty(input, 'files', { value: [file] });

    fixture.componentInstance.selectImage({ target: input } as unknown as Event);
    fixture.componentInstance.uploadImage();

    expect(accountApi.updateProfileImage).not.toHaveBeenCalled();
    expect(fixture.componentInstance.errorMessage()).toContain('PNG');
  });

  it('requires the password and exact confirmation before closing the owned account', () => {
    const fixture = TestBed.createComponent(AccountComponent);
    fixture.detectChanges();

    fixture.componentInstance.openDeleteConfirmation();
    fixture.componentInstance.deleteAccountForm.setValue({
      password: 'Password123!',
      confirmation: 'حذف',
    });
    fixture.componentInstance.deleteAccount();
    expect(accountApi.deleteAccount).not.toHaveBeenCalled();

    fixture.componentInstance.deleteAccountForm.controls.confirmation.setValue('حذف حسابي');
    fixture.componentInstance.deleteAccount();

    expect(accountApi.deleteAccount).toHaveBeenCalledWith('Password123!');
    expect(session.clearSession).toHaveBeenCalledOnce();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { accountDeleted: 'true' },
    });
  });
});

function profile() {
  return {
    id: '1',
    displayName: 'يوسف أحمد',
    email: 'user@test.com',
    imageUrl: null,
    userName: 'user@test.com',
    phoneNumber: '+201001234567',
  };
}
