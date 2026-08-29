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
        { provide: AuthApiService, useValue: authApi as Partial<AuthApiService> }
      ]
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
      password: '12345678'
    });

    component.submit();

    expect(component.form.controls.displayName.errors).toEqual({ arabicName: true });
    expect(authApi.register).not.toHaveBeenCalled();
  });

  it('navigates to confirm email after successful registration', () => {
    authApi.register.mockReturnValue(
      of({
        statusCode: 200,
        message: 'تم إنشاء الحساب'
      })
    );

    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    component.form.setValue({
      displayName: 'محمد أحمد',
      email: 'user@test.com',
      password: '12345678'
    });

    component.submit();

    expect(authApi.register).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/confirm-email'], {
      queryParams: { email: 'user@test.com', registered: true }
    });
  });
});
