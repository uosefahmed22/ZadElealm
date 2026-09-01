import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AuthApiService } from '../../../core/auth/auth-api.service';
import { ResetPasswordComponent } from './reset-password.component';

describe('ResetPasswordComponent', () => {
  let resetPassword: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    resetPassword = vi.fn();
    TestBed.configureTestingModule({
      imports: [ResetPasswordComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParamMap: convertToParamMap({ email: 'student@test.com' }) },
          },
        },
        { provide: AuthApiService, useValue: { resetPassword } },
      ],
    });
  });

  it('keeps the action enabled and explains invalid passwords after click', () => {
    const fixture = TestBed.createComponent(ResetPasswordComponent);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;

    expect(button.disabled).toBe(false);
    button.click();
    fixture.detectChanges();

    expect(resetPassword).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('كلمة المرور يجب ألا تقل عن 8 أحرف');
    expect(fixture.nativeElement.textContent).toContain('أكد كلمة المرور الجديدة');
  });

  it('submits matching passwords and returns to login', () => {
    resetPassword.mockReturnValue(of({ statusCode: 200, message: 'تم التغيير' }));
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(ResetPasswordComponent);
    fixture.componentInstance.form.setValue({
      email: 'student@test.com',
      newPassword: 'Password123!',
      confirmPassword: 'Password123!',
    });

    fixture.componentInstance.submit();

    expect(resetPassword).toHaveBeenCalledWith({
      email: 'student@test.com',
      newPassword: 'Password123!',
      confirmPassword: 'Password123!',
    });
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
