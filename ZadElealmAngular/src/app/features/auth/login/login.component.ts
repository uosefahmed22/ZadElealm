import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { normalizeApiError } from '../../../core/api/api-error.utils';
import { AuthApiService } from '../../../core/auth/auth-api.service';
import { AuthSessionService } from '../../../core/auth/auth-session.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly session = inject(AuthSessionService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly hidePassword = signal(true);
  readonly isSubmitting = signal(false);
  readonly successMessage = signal(this.initialSuccessMessage());
  readonly serverMessage = signal('');
  readonly serverErrors = signal<string[]>([]);
  readonly needsEmailConfirmation = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.serverMessage.set('');
    this.serverErrors.set([]);
    this.needsEmailConfirmation.set(false);

    this.authApi
      .login(this.form.getRawValue())
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.session.setSession(response.data);
          this.successMessage.set('تم تسجيل الدخول بنجاح.');
          const returnUrl = this.router.parseUrl(this.getSafeReturnUrl());
          void this.router.navigateByUrl(returnUrl);
        },
        error: (error: unknown) => {
          const normalized = normalizeApiError(error);
          this.serverMessage.set(normalized.message);
          this.serverErrors.set(normalized.errors);
          this.needsEmailConfirmation.set(
            normalized.status === 401 && normalized.message.includes('تأكيد'),
          );
        },
      });
  }

  private getSafeReturnUrl(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    return returnUrl?.startsWith('/') && !returnUrl.startsWith('//') ? returnUrl : '/app';
  }

  togglePasswordVisibility(): void {
    this.hidePassword.update((value) => !value);
  }

  private initialSuccessMessage(): string {
    if (this.route.snapshot.queryParamMap.get('accountDeleted') === 'true') {
      return 'تم إغلاق حسابك بنجاح، ولن تتمكن من تسجيل الدخول إليه مرة أخرى.';
    }
    if (this.route.snapshot.queryParamMap.get('emailChanged') === 'true') {
      return 'تم تحديث البريد الإلكتروني. سجّل الدخول بالبريد الجديد.';
    }
    return '';
  }

  getError(controlName: 'email' | 'password'): string {
    const control = this.form.controls[controlName];
    if (!control.touched || !control.errors) {
      return '';
    }

    if (control.errors['required']) {
      return 'هذا الحقل مطلوب.';
    }

    if (control.errors['email']) {
      return 'أدخل بريدًا إلكترونيًا صحيحًا.';
    }

    return 'البيانات المدخلة غير صحيحة.';
  }
}
