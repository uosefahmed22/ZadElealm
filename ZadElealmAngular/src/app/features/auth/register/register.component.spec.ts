import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { AuthApiService } from '../../../core/auth/auth-api.service';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let authApi: { register: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(async () => {
    authApi = { register: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: new Map() } } },
        { provide: AuthApiService, useValue: authApi as Partial<AuthApiService> },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  it('rejects non-Arabic names on the client before calling the api', () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    component.form.setValue({
      displayName: 'Test User',
      email: 'user@test.com',
      password: '12345678',
    });

    component.submit();

    expect(component.form.controls.displayName.errors).toEqual({ arabicName: true });
    expect(authApi.register).not.toHaveBeenCalled();
  });

  it('keeps the register button actionable so invalid fields can explain the problem', () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    expect(button.disabled).toBe(false);
    button.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('هذا الحقل مطلوب');
  });

  it('navigates to confirm email after successful registration', () => {
    authApi.register.mockReturnValue(
      of({
        statusCode: 200,
        message: 'تم إنشاء الحساب',
      }),
    );

    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    component.form.setValue({
      displayName: 'محمد أحمد',
      email: 'user@test.com',
      password: '12345678',
    });

    component.submit();

    expect(authApi.register).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/confirm-email'], {
      queryParams: { email: 'user@test.com', registered: true },
    });
  });

  it('shows password strength without changing the existing validator contract', () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;

    component.form.controls.password.setValue('12345678');
    fixture.detectChanges();
    expect(component.passwordStrength()).toBe(2);

    component.form.controls.password.setValue('Abcd1234');
    fixture.detectChanges();
    expect(component.passwordStrength()).toBe(3);
    expect(component.form.controls.password.valid).toBe(true);
    expect(
      fixture.nativeElement.querySelector('.password-strength').getAttribute('data-strength'),
    ).toBe('3');
  });
});
