import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { CertificateDto } from '../../core/assessments/assessment.models';
import { formatArabicDate } from '../../core/i18n/arabic-number-format.util';

@Component({
  selector: 'app-certificates',
  imports: [RouterLink],
  templateUrl: './certificates.component.html',
  styleUrl: './certificates.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CertificatesComponent implements OnInit {
  private readonly api = inject(AssessmentApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly certificates = signal<readonly CertificateDto[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly fileErrorMessage = signal('');
  readonly pendingCertificateIds = signal<ReadonlySet<number>>(new Set());

  ngOnInit(): void {
    this.loadCertificates();
  }

  retry(): void {
    this.loadCertificates();
  }

  formatArabicDate(value: string): string {
    return formatArabicDate(value);
  }

  certificateTitle(_certificate: CertificateDto): string {
    return 'شهادة اجتياز';
  }

  certificateDescription(certificate: CertificateDto): string {
    const description = certificate.description.trim();
    return description && !/[A-Za-z]/.test(description)
      ? description
      : `شهادة إتمام ${certificate.quizName} بنجاح`;
  }

  openCertificate(certificate: CertificateDto): void {
    if (this.pendingCertificateIds().has(certificate.id)) return;

    const viewer = window.open('about:blank', '_blank');
    if (!viewer) {
      this.fileErrorMessage.set('تعذر فتح نافذة الشهادة. اسمح بالنوافذ المنبثقة ثم حاول مجددًا.');
      return;
    }

    viewer.opener = null;
    this.setCertificatePending(certificate.id, true);
    this.fileErrorMessage.set('');

    this.api
      .downloadCertificate(certificate.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (pdf) => {
          const objectUrl = URL.createObjectURL(pdf);
          viewer.location.replace(objectUrl);
          window.setTimeout(() => URL.revokeObjectURL(objectUrl), 60_000);
          this.setCertificatePending(certificate.id, false);
        },
        error: (error: unknown) => {
          viewer.close();
          this.setCertificatePending(certificate.id, false);
          this.fileErrorMessage.set(normalizeApiError(error).message);
        },
      });
  }

  private loadCertificates(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.api
      .getCertificates()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.certificates.set(response.data);
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(normalizeApiError(error).message);
          this.isLoading.set(false);
        },
      });
  }

  private setCertificatePending(certificateId: number, pending: boolean): void {
    const nextIds = new Set(this.pendingCertificateIds());
    pending ? nextIds.add(certificateId) : nextIds.delete(certificateId);
    this.pendingCertificateIds.set(nextIds);
  }
}
