import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AuthApiService } from '../../../core/auth/auth-api.service';
import { ForgotPasswordComponent } from './forgot-password.component';

describe('ForgotPasswordComponent', () => {
  let forgotPassword: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    forgotPassword = vi.fn();
    TestBed.configureTestingModule({
      imports: [ForgotPasswordComponent],
      providers: [provideRouter([]), { provide: AuthApiService, useValue: { forgotPassword } }],
    });
  });

  it('keeps the action enabled and explains an invalid email after click', () => {
    const fixture = TestBed.createComponent(ForgotPasswordComponent);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;

    expect(button.disabled).toBe(false);
    button.click();
    fixture.detectChanges();

    expect(forgotPassword).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('أدخل بريدًا إلكترونيًا صحيحًا');
  });

  it('sends the email and moves to OTP verification', () => {
    forgotPassword.mockReturnValue(of({ statusCode: 200, message: 'تم إرسال الرمز' }));
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(ForgotPasswordComponent);
    fixture.componentInstance.form.setValue({ email: 'student@test.com' });

    fixture.componentInstance.submit();

    expect(forgotPassword).toHaveBeenCalledWith('student@test.com');
    expect(router.navigate).toHaveBeenCalledWith(['/verify-otp'], {
      queryParams: { email: 'student@test.com' },
    });
  });
});
