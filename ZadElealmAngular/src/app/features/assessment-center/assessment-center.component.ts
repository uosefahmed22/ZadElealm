import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { AssessmentSummaryDto } from '../../core/assessments/assessment.models';

@Component({
  selector: 'app-assessment-center',
  imports: [CommonModule, RouterLink],
  templateUrl: './assessment-center.component.html',
  styleUrl: './assessment-center.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssessmentCenterComponent implements OnInit {
  private readonly api = inject(AssessmentApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly assessments = signal<AssessmentSummaryDto[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.load();
  }

  retry(): void {
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.api
      .getAssessments()
      .pipe(finalize(() => this.isLoading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => this.assessments.set(response.data),
        error: (error: unknown) => this.errorMessage.set(normalizeApiError(error).message),
      });
  }
}
