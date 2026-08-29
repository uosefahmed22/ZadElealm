import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { normalizeApiError } from '../../../core/api/api-error.utils';
import { AuthApiService } from '../../../core/auth/auth-api.service';
import { appEnvironment } from '../../../core/config/app-environment';

@Component({
  selector: 'app-confirm-email',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './confirm-email.component.html',
  styleUrl: './confirm-email.component.scss'
})
export class ConfirmEmailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);

  readonly justLoggedIn = signal(this.route.snapshot.queryParamMap.get('justLoggedIn') === 'true');
  readonly registered = signal(this.route.snapshot.queryParamMap.get('registered') === 'true');
  readonly email = signal(this.route.snapshot.queryParamMap.get('email') ?? '');
  readonly confirmationUrl = computed(() => {
    const userId = this.route.snapshot.queryParamMap.get('userId');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!userId || !token) {
      return '';
    }

    const encodedUserId = encodeURIComponent(userId);
    const encodedToken = encodeURIComponent(token);
    return `${appEnvironment.apiBaseUrl}/Account/confirm-email?userId=${encodedUserId}&token=${encodedToken}`;
  });

  readonly isSubmitting = signal(false);
  readonly successMessage = signal('');
  readonly serverMessage = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    email: [this.email(), [Validators.required, Validators.email]]
  });

  resend(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.successMessage.set('');
    this.serverMessage.set('');

    this.authApi
      .resendConfirmationEmail(this.form.controls.email.value)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response) => {
          this.successMessage.set(response.message ?? 'تمت إعادة إرسال رسالة التفعيل.');
        },
        error: (error: unknown) => {
          this.serverMessage.set(normalizeApiError(error).message);
        }
      });
  }
}
