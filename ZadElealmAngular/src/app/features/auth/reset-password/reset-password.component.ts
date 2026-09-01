import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { normalizeApiError } from '../../../core/api/api-error.utils';
import { AuthApiService } from '../../../core/auth/auth-api.service';
import { passwordMatchValidator } from '../../../shared/validators/password-match.validator';

@Component({
  selector: 'app-reset-password',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly email = signal(this.route.snapshot.queryParamMap.get('email') ?? '');
  readonly hidePassword = signal(true);
  readonly hideConfirmPassword = signal(true);
  readonly isSubmitting = signal(false);
  readonly successMessage = signal('');
  readonly serverMessage = signal('');

  readonly form = this.formBuilder.nonNullable.group(
    {
      email: [this.email(), [Validators.required, Validators.email]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: [passwordMatchValidator('newPassword', 'confirmPassword')] },
  );

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.serverMessage.set('');

    this.authApi
      .resetPassword(this.form.getRawValue())
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.successMessage.set(response.message ?? 'تم تغيير كلمة المرور بنجاح.');
          void this.router.navigate(['/login']);
        },
        error: (error: unknown) => {
          this.serverMessage.set(normalizeApiError(error).message);
        },
      });
  }

  get confirmPasswordMessage(): string {
    return this.form.hasError('passwordMismatch') && this.form.controls.confirmPassword.touched
      ? 'كلمتا المرور غير متطابقتين.'
      : '';
  }
}
