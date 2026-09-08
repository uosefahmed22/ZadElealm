import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { normalizeApiError } from '../../../core/api/api-error.utils';
import { AuthApiService } from '../../../core/auth/auth-api.service';
import { normalizeArabicDigits } from '../../../core/i18n/arabic-number-format.util';

@Component({
  selector: 'app-verify-otp',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './verify-otp.component.html',
  styleUrl: './verify-otp.component.scss',
})
export class VerifyOtpComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly email = signal(this.route.snapshot.queryParamMap.get('email') ?? '');
  readonly isSubmitting = signal(false);
  readonly successMessage = signal('');
  readonly serverMessage = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    email: [this.email(), [Validators.required, Validators.email]],
    otp: ['', [Validators.required, Validators.pattern(/^[0-9٠-٩]{4,8}$/)]],
  });

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, otp } = this.form.getRawValue();
    this.isSubmitting.set(true);
    this.serverMessage.set('');

    this.authApi
      .verifyOtp(email, normalizeArabicDigits(otp))
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.successMessage.set(response.message ?? 'تم التحقق من الرمز.');
          void this.router.navigate(['/reset-password'], { queryParams: { email } });
        },
        error: (error: unknown) => {
          this.serverMessage.set(normalizeApiError(error).message);
        },
      });
  }
}
