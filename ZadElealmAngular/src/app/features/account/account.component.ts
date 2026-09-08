import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { AccountApiService } from '../../core/account/account-api.service';
import { UserProfileDto } from '../../core/account/account.models';
import { normalizeApiError } from '../../core/api/api-error.utils';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import { normalizeArabicDigits } from '../../core/i18n/arabic-number-format.util';

const arabicNamePattern = /^[\u0600-\u06ff\s]+$/;
const acceptedImageTypes = new Set(['image/jpeg', 'image/png']);
const maxImageBytes = 5 * 1024 * 1024;
type AccountAction = 'profile' | 'password' | 'image' | 'email' | 'delete';

@Component({
  selector: 'app-account',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './account.component.html',
  styleUrl: './account.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly accountApi = inject(AccountApiService);
  private readonly session = inject(AuthSessionService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly profile = signal<UserProfileDto | null>(null);
  readonly isLoading = signal(true);
  readonly loadError = signal('');
  readonly activeAction = signal<AccountAction | null>(null);
  readonly statusMessage = signal('');
  readonly errorMessage = signal('');
  readonly errorAction = signal<AccountAction | null>(null);
  readonly imageError = signal(false);
  readonly emailOtpSent = signal(false);
  readonly pendingEmail = signal('');
  readonly selectedImage = signal<File | null>(null);
  readonly selectedImageName = computed(() => this.selectedImage()?.name ?? '');
  readonly deleteConfirmationOpen = signal(false);
  readonly deleteConfirmationText = 'حذف حسابي';
  readonly initials = computed(() => {
    const name = this.profile()?.displayName.trim();
    return name
      ? name
          .split(/\s+/)
          .slice(0, 2)
          .map((part) => part[0])
          .join('')
      : 'ز';
  });

  readonly profileForm = this.formBuilder.nonNullable.group({
    displayName: [
      '',
      [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100),
        Validators.pattern(arabicNamePattern),
      ],
    ],
    phoneNumber: [''],
  });

  readonly passwordForm = this.formBuilder.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  });

  readonly emailForm = this.formBuilder.nonNullable.group({
    newEmail: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    token: ['', [Validators.required, Validators.pattern(/^[0-9٠-٩]{6}$/)]],
  });

  readonly deleteAccountForm = this.formBuilder.nonNullable.group({
    password: ['', Validators.required],
    confirmation: ['', Validators.required],
  });

  ngOnInit(): void {
    this.loadProfile();
  }

  retry(): void {
    this.loadProfile();
  }

  saveProfile(): void {
    if (this.profileForm.invalid || this.activeAction()) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.beginAction('profile');
    const value = this.profileForm.getRawValue();
    this.accountApi
      .updateProfile({
        displayName: value.displayName.trim(),
        phoneNumber: value.phoneNumber.trim() || null,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.activeAction.set(null)),
      )
      .subscribe({
        next: (response) => {
          const displayName = value.displayName.trim();
          this.profile.update((profile) =>
            profile
              ? { ...profile, displayName, phoneNumber: value.phoneNumber.trim() || null }
              : profile,
          );
          this.session.updateIdentity(
            displayName,
            this.profile()?.email ?? this.session.user()?.email ?? '',
          );
          this.statusMessage.set(response.message ?? 'تم حفظ بياناتك بنجاح.');
        },
        error: (error: unknown) => this.setError(error),
      });
  }

  changePassword(): void {
    const value = this.passwordForm.getRawValue();
    if (
      this.passwordForm.invalid ||
      value.newPassword !== value.confirmPassword ||
      this.activeAction()
    ) {
      this.passwordForm.markAllAsTouched();
      if (value.newPassword !== value.confirmPassword) {
        this.setActionError('password', 'تأكيد كلمة المرور غير مطابق.');
      }
      return;
    }

    this.beginAction('password');
    this.accountApi
      .changePassword({ currentPassword: value.currentPassword, newPassword: value.newPassword })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.activeAction.set(null)),
      )
      .subscribe({
        next: (response) => {
          this.passwordForm.reset();
          this.statusMessage.set(response.message ?? 'تم تغيير كلمة المرور بنجاح.');
        },
        error: (error: unknown) => this.setError(error),
      });
  }

  selectImage(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.clearMessages();
    if (!file) {
      this.selectedImage.set(null);
      return;
    }
    if (!acceptedImageTypes.has(file.type)) {
      this.setActionError('image', 'اختر صورة بصيغة PNG أو JPG فقط.');
      input.value = '';
      return;
    }
    if (file.size > maxImageBytes) {
      this.setActionError('image', 'حجم الصورة يجب ألا يتجاوز ٥ ميجابايت.');
      input.value = '';
      return;
    }
    this.selectedImage.set(file);
  }

  uploadImage(): void {
    const file = this.selectedImage();
    if (!file || this.activeAction()) return;
    this.beginAction('image');
    this.accountApi
      .updateProfileImage(file)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.activeAction.set(null)),
      )
      .subscribe({
        next: (response) => {
          this.selectedImage.set(null);
          this.statusMessage.set(response.message ?? 'تم تحديث الصورة الشخصية.');
          this.loadProfile(false);
        },
        error: (error: unknown) => this.setError(error),
      });
  }

  removeImage(): void {
    if (this.activeAction()) return;
    this.beginAction('image');
    this.accountApi
      .removeProfileImage()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.activeAction.set(null)),
      )
      .subscribe({
        next: (response) => {
          this.profile.update((profile) => (profile ? { ...profile, imageUrl: null } : profile));
          this.imageError.set(false);
          this.statusMessage.set(response.message ?? 'تم حذف الصورة الشخصية.');
        },
        error: (error: unknown) => this.setError(error),
      });
  }

  sendEmailOtp(): void {
    const { newEmail, password } = this.emailForm.getRawValue();
    const normalizedNewEmail = newEmail.trim().toLowerCase();
    const currentEmail = this.profile()?.email.trim().toLowerCase();
    if (
      this.emailForm.controls.newEmail.invalid ||
      this.emailForm.controls.password.invalid ||
      this.activeAction()
    ) {
      this.emailForm.controls.newEmail.markAsTouched();
      this.emailForm.controls.password.markAsTouched();
      return;
    }

    if (normalizedNewEmail === currentEmail) {
      this.emailForm.controls.newEmail.markAsTouched();
      this.setActionError('email', 'البريد الجديد هو نفس بريدك الحالي. أدخل بريدًا مختلفًا.');
      return;
    }

    this.beginAction('email');
    this.accountApi
      .sendEmailOtp({ newEmail: normalizedNewEmail, password })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.activeAction.set(null)),
      )
      .subscribe({
        next: (response) => {
          this.pendingEmail.set(normalizedNewEmail);
          this.emailOtpSent.set(true);
          this.statusMessage.set(response.message ?? 'أرسلنا رمز التحقق إلى البريد الجديد.');
        },
        error: (error: unknown) => this.setError(error),
      });
  }

  confirmEmail(): void {
    if (!this.emailOtpSent() || this.emailForm.controls.token.invalid || this.activeAction()) {
      this.emailForm.controls.token.markAsTouched();
      return;
    }

    this.beginAction('email');
    this.accountApi
      .updateEmail({
        newEmail: this.pendingEmail(),
        token: normalizeArabicDigits(this.emailForm.controls.token.value),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.activeAction.set(null)),
      )
      .subscribe({
        next: () => {
          this.session.clearSession();
          void this.router.navigate(['/login'], { queryParams: { emailChanged: 'true' } });
        },
        error: (error: unknown) => this.setError(error),
      });
  }

  resetEmailChange(): void {
    this.emailOtpSent.set(false);
    this.pendingEmail.set('');
    this.emailForm.reset();
    this.clearMessages();
  }

  openDeleteConfirmation(): void {
    if (this.activeAction()) return;
    this.clearMessages();
    this.deleteAccountForm.reset();
    this.deleteConfirmationOpen.set(true);
  }

  closeDeleteConfirmation(): void {
    if (this.activeAction() === 'delete') return;
    this.deleteConfirmationOpen.set(false);
    this.deleteAccountForm.reset();
  }

  deleteAccount(): void {
    const value = this.deleteAccountForm.getRawValue();
    if (
      this.deleteAccountForm.invalid ||
      value.confirmation.trim() !== this.deleteConfirmationText ||
      this.activeAction()
    ) {
      this.deleteAccountForm.markAllAsTouched();
      if (value.confirmation.trim() !== this.deleteConfirmationText) {
        this.setActionError('delete', `اكتب «${this.deleteConfirmationText}» كما هي للتأكيد.`);
      }
      return;
    }

    this.beginAction('delete');
    this.accountApi
      .deleteAccount(value.password)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.activeAction.set(null)),
      )
      .subscribe({
        next: () => {
          this.session.clearSession();
          void this.router.navigate(['/login'], { queryParams: { accountDeleted: 'true' } });
        },
        error: (error: unknown) => this.setError(error),
      });
  }

  private loadProfile(showLoader = true): void {
    if (showLoader) this.isLoading.set(true);
    this.loadError.set('');
    this.accountApi
      .getProfile()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.profile.set(response.data);
          this.profileForm.setValue({
            displayName: response.data.displayName,
            phoneNumber: response.data.phoneNumber ?? '',
          });
          this.emailForm.controls.newEmail.setValue('');
          this.imageError.set(false);
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.isLoading.set(false);
          this.loadError.set(normalizeApiError(error).message);
        },
      });
  }

  private beginAction(action: AccountAction): void {
    this.clearMessages();
    this.activeAction.set(action);
  }

  private clearMessages(): void {
    this.statusMessage.set('');
    this.errorMessage.set('');
    this.errorAction.set(null);
  }

  private setError(error: unknown): void {
    this.setActionError(this.activeAction() ?? 'profile', normalizeApiError(error).message);
  }

  private setActionError(action: AccountAction, message: string): void {
    this.statusMessage.set('');
    this.errorAction.set(action);
    this.errorMessage.set(message);
  }
}
