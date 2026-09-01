import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AuthApiService } from '../../../core/auth/auth-api.service';
import { VerifyOtpComponent } from './verify-otp.component';

describe('VerifyOtpComponent', () => {
  let verifyOtp: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    verifyOtp = vi.fn();
    TestBed.configureTestingModule({
      imports: [VerifyOtpComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParamMap: convertToParamMap({ email: 'student@test.com' }) },
          },
        },
        { provide: AuthApiService, useValue: { verifyOtp } },
      ],
    });
  });

  it('keeps the action enabled and explains a missing OTP after click', () => {
    const fixture = TestBed.createComponent(VerifyOtpComponent);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;

    expect(button.disabled).toBe(false);
    button.click();
    fixture.detectChanges();

    expect(verifyOtp).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('أدخل رمز التحقق كما وصل إليك');
  });

  it('verifies the OTP and moves to password reset', () => {
    verifyOtp.mockReturnValue(of({ statusCode: 200, message: 'تم التحقق' }));
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(VerifyOtpComponent);
    fixture.componentInstance.form.setValue({ email: 'student@test.com', otp: '1234' });

    fixture.componentInstance.submit();

    expect(verifyOtp).toHaveBeenCalledWith('student@test.com', '1234');
    expect(router.navigate).toHaveBeenCalledWith(['/reset-password'], {
      queryParams: { email: 'student@test.com' },
    });
  });
});
