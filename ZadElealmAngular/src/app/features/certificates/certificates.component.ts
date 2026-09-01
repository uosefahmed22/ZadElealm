import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { CertificateDto } from '../../core/assessments/assessment.models';
import { appEnvironment } from '../../core/config/app-environment';

@Component({
  selector: 'app-certificates',
  imports: [DatePipe, RouterLink],
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

  ngOnInit(): void {
    this.loadCertificates();
  }

  retry(): void {
    this.loadCertificates();
  }

  certificateUrl(pdfUrl: string): string {
    if (/^https?:\/\//i.test(pdfUrl)) return pdfUrl;
    const apiOrigin = new URL(appEnvironment.apiBaseUrl).origin;
    return new URL(pdfUrl.replace(/^\//, ''), `${apiOrigin}/`).toString();
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
}
