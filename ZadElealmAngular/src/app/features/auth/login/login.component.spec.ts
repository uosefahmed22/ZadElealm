import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';

import { AuthApiService } from '../../../core/auth/auth-api.service';
import { AuthSessionService } from '../../../core/auth/auth-session.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let authApi: { login: ReturnType<typeof vi.fn> };
  let session: { setSession: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(async () => {
    authApi = { login: vi.fn() };
    session = { setSession: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({}) } },
        },
        { provide: AuthApiService, useValue: authApi as Partial<AuthApiService> },
        { provide: AuthSessionService, useValue: session as Partial<AuthSessionService> },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
  });

  it('marks controls as touched when submitting an invalid form', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;

    component.submit();

    expect(component.form.controls.email.touched).toBe(true);
    expect(authApi.login).not.toHaveBeenCalled();
  });

  it('keeps the login button actionable so invalid fields can explain the problem', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    expect(button.disabled).toBe(false);
    button.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('هذا الحقل مطلوب');
  });

  it('stores the session and navigates after a successful login', () => {
    authApi.login.mockReturnValue(
      of({
        statusCode: 200,
        data: {
          displayName: 'طالب',
          email: 'user@test.com',
          token: 'access',
          refreshToken: 'refresh',
        },
      }),
    );

    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'user@test.com', password: '12345678' });

    component.submit();

    expect(session.setSession).toHaveBeenCalled();
    expect(router.navigateByUrl).toHaveBeenCalledWith(
      expect.objectContaining({ toString: expect.any(Function) }),
    );
    expect(router.navigateByUrl).toHaveBeenCalledTimes(1);
  });

  it('shows server errors when login fails', () => {
    authApi.login.mockReturnValue(throwError(() => new Error('ignored')));

    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'user@test.com', password: '12345678' });

    component.submit();

    expect(component.serverMessage()).toContain('ignored');
  });

  it('offers the confirmation route only when the API reports an unconfirmed email', () => {
    authApi.login.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 401,
            error: { statusCode: 401, message: 'لم يتم تأكيد البريد الإلكتروني' },
          }),
      ),
    );
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.componentInstance.form.setValue({
      email: 'student@test.com',
      password: 'Password123!',
    });

    fixture.componentInstance.submit();
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('.confirmation-link') as HTMLAnchorElement;
    expect(link).not.toBeNull();
    expect(link.getAttribute('href')).toContain('/confirm-email');
    expect(link.getAttribute('href')).toContain('email=student@test.com');
  });

  it('exposes accessible password visibility and server alert states', () => {
    authApi.login.mockReturnValue(throwError(() => new Error('بيانات غير صحيحة')));
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    component.form.setValue({ email: 'user@test.com', password: '12345678' });
    fixture.detectChanges();

    const toggle = fixture.nativeElement.querySelector('.password-toggle') as HTMLButtonElement;
    expect(toggle.getAttribute('aria-label')).toBe('إظهار كلمة المرور');
    toggle.click();
    fixture.detectChanges();
    expect(toggle.getAttribute('aria-label')).toBe('إخفاء كلمة المرور');

    component.submit();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();
  });
});
