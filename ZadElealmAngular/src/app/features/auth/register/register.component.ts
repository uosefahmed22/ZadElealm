import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { normalizeApiError } from '../../../core/api/api-error.utils';
import { AuthApiService } from '../../../core/auth/auth-api.service';
import { arabicNameValidator } from '../../../shared/validators/arabic-name.validator';

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);

  readonly hidePassword = signal(true);
  readonly isSubmitting = signal(false);
  readonly successMessage = signal('');
  readonly serverMessage = signal('');
  readonly serverErrors = signal<string[]>([]);

  readonly form = this.formBuilder.nonNullable.group({
    displayName: ['', [Validators.required, arabicNameValidator()]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  readonly submitDisabled = computed(() => this.isSubmitting() || this.form.invalid);

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.serverMessage.set('');
    this.serverErrors.set([]);

    this.authApi
      .register(this.form.getRawValue())
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.successMessage.set(response.message ?? 'تم إنشاء الحساب بنجاح.');
          void this.router.navigate(['/confirm-email'], {
            queryParams: { email: this.form.controls.email.value, registered: true }
          });
        },
        error: (error: unknown) => {
          const normalized = normalizeApiError(error);
          this.serverMessage.set(normalized.message);
          this.serverErrors.set(normalized.errors);
        }
      });
  }

  togglePasswordVisibility(): void {
    this.hidePassword.update((value) => !value);
  }

  getError(controlName: 'displayName' | 'email' | 'password'): string {
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

    if (control.errors['minlength']) {
      return 'كلمة المرور يجب ألا تقل عن 8 أحرف.';
    }

    if (control.errors['arabicName']) {
      return 'الاسم يجب أن يكون باللغة العربية فقط.';
    }

    return 'البيانات المدخلة غير صحيحة.';
  }
}
