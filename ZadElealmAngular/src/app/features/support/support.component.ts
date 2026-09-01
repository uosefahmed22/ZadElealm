import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { SupportApiService } from '../../core/support/support-api.service';
import { ReportType, reportTypeOptions } from '../../core/support/support.models';

@Component({
  selector: 'app-support',
  imports: [ReactiveFormsModule],
  templateUrl: './support.component.html',
  styleUrl: './support.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SupportComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly supportApi = inject(SupportApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly reportTypes = reportTypeOptions;
  readonly isSubmitting = signal(false);
  readonly successMessage = signal('');
  readonly errorMessage = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    reportType: ['Technical' as ReportType, Validators.required],
    titleOfTheIssue: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]],
  });

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.successMessage.set('');
    this.errorMessage.set('');
    const value = this.form.getRawValue();
    this.supportApi
      .createReport({
        reportType: value.reportType,
        titleOfTheIssue: value.titleOfTheIssue.trim(),
        description: value.description.trim(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false)),
      )
      .subscribe({
        next: (response) => {
          this.form.reset({ reportType: 'Technical', titleOfTheIssue: '', description: '' });
          this.successMessage.set(response.message ?? 'تم إرسال البلاغ بنجاح.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(normalizeApiError(error).message);
        },
      });
  }

  titleError(): string {
    const control = this.form.controls.titleOfTheIssue;
    if (!control.touched || !control.errors) return '';
    if (control.errors['required']) return 'اكتب عنوانًا مختصرًا للمشكلة.';
    if (control.errors['maxlength']) return 'العنوان يجب ألا يتجاوز 100 حرف.';
    return 'راجع عنوان المشكلة.';
  }

  descriptionError(): string {
    const control = this.form.controls.description;
    if (!control.touched || !control.errors) return '';
    if (control.errors['required']) return 'اكتب تفاصيل المشكلة.';
    if (control.errors['minlength']) return 'الوصف يجب أن يكون 10 أحرف على الأقل.';
    if (control.errors['maxlength']) return 'الوصف يجب ألا يتجاوز 1000 حرف.';
    return 'راجع وصف المشكلة.';
  }
}
