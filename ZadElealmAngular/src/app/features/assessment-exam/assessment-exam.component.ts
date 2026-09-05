import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { AssessmentResultDto, CategoryAssessmentDto } from '../../core/assessments/assessment.models';

@Component({
  selector: 'app-assessment-exam',
  imports: [CommonModule, RouterLink],
  templateUrl: './assessment-exam.component.html',
  styleUrl: '../quiz/quiz.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssessmentExamComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AssessmentApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly assessmentId = Number(this.route.snapshot.paramMap.get('assessmentId'));
  readonly assessment = signal<CategoryAssessmentDto | null>(null);
  readonly answers = signal<Readonly<Record<number, number>>>({});
  readonly result = signal<AssessmentResultDto | null>(null);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');
  readonly answeredCount = computed(() => Object.keys(this.answers()).length);
  readonly allAnswered = computed(() => {
    const total = this.assessment()?.questions.length ?? 0;
    return total > 0 && this.answeredCount() === total;
  });
  readonly submitDisabled = computed(() => !this.allAnswered() || this.isSubmitting());

  ngOnInit(): void { this.load(); }

  selectAnswer(questionId: number, choiceId: number): void {
    if (!this.isSubmitting()) this.answers.update(answers => ({ ...answers, [questionId]: choiceId }));
  }

  submit(): void {
    const assessment = this.assessment();
    if (!assessment || this.submitDisabled()) return;
    this.isSubmitting.set(true);
    this.errorMessage.set('');
    this.api.submitAssessment(this.assessmentId, {
      studentAnswers: assessment.questions.map(question => ({
        questionId: question.id,
        choiceId: this.answers()[question.id],
      })),
    }).pipe(finalize(() => this.isSubmitting.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => this.result.set(response.data),
        error: (error: unknown) => this.errorMessage.set(normalizeApiError(error).message),
      });
  }

  retryAssessment(): void {
    this.result.set(null);
    this.answers.set({});
    this.errorMessage.set('');
  }

  retryLoad(): void { this.load(); }

  private load(): void {
    if (!Number.isInteger(this.assessmentId) || this.assessmentId <= 0) {
      this.isLoading.set(false);
      this.errorMessage.set('رابط الاختبار غير صالح.');
      return;
    }
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.api.getAssessment(this.assessmentId)
      .pipe(finalize(() => this.isLoading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => this.assessment.set(response.data),
        error: (error: unknown) => this.errorMessage.set(normalizeApiError(error).message),
      });
  }
}
