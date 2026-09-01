import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthApiService } from '../../../core/auth/auth-api.service';
import { ConfirmEmailComponent } from './confirm-email.component';

describe('ConfirmEmailComponent', () => {
  it('prefills the email and resends through the real auth service contract', () => {
    const resendConfirmationEmail = vi.fn(() =>
      of({ statusCode: 200, message: 'تم إرسال رسالة التأكيد بنجاح' }),
    );
    TestBed.configureTestingModule({
      imports: [ConfirmEmailComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: convertToParamMap({ email: 'student@test.com', registered: 'true' }),
            },
          },
        },
        { provide: AuthApiService, useValue: { resendConfirmationEmail } },
      ],
    });

    const fixture = TestBed.createComponent(ConfirmEmailComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.form.controls.email.value).toBe('student@test.com');

    fixture.componentInstance.resend();
    fixture.detectChanges();

    expect(resendConfirmationEmail).toHaveBeenCalledWith('student@test.com');
    expect(fixture.nativeElement.textContent).toContain('تم إرسال رسالة التأكيد بنجاح');
  });

  it('keeps the action enabled and explains an invalid email after click', () => {
    const resendConfirmationEmail = vi.fn(() =>
      throwError(() => new Error('should not be called')),
    );
    TestBed.configureTestingModule({
      imports: [ConfirmEmailComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({}) } },
        },
        { provide: AuthApiService, useValue: { resendConfirmationEmail } },
      ],
    });

    const fixture = TestBed.createComponent(ConfirmEmailComponent);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    expect(button.disabled).toBe(false);

    button.click();
    fixture.detectChanges();

    expect(resendConfirmationEmail).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('أدخل بريدًا إلكترونيًا صحيحًا');
  });
});
