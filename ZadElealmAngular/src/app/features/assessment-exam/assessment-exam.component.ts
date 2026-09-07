import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, interval } from 'rxjs';

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
  readonly showSubmitConfirmation = signal(false);
  readonly remainingSeconds = signal(0);
  readonly errorMessage = signal('');
  readonly answeredCount = computed(() => Object.keys(this.answers()).length);
  readonly allAnswered = computed(() => {
    const total = this.assessment()?.questions.length ?? 0;
    return total > 0 && this.answeredCount() === total;
  });
  readonly timeExpired = computed(() => this.remainingSeconds() <= 0);
  readonly submitDisabled = computed(() => !this.allAnswered() || this.isSubmitting() || this.timeExpired());
  readonly formattedRemainingTime = computed(() => {
    const seconds = Math.max(0, this.remainingSeconds());
    const minutesPart = Math.floor(seconds / 60).toString().padStart(2, '0');
    const secondsPart = (seconds % 60).toString().padStart(2, '0');
    return `${minutesPart}:${secondsPart}`;
  });
  private autoSubmitStarted = false;

  ngOnInit(): void {
    this.load();
    interval(1000)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.updateTimer());
  }

  selectAnswer(questionId: number, choiceId: number): void {
    if (!this.isSubmitting()) this.answers.update(answers => ({ ...answers, [questionId]: choiceId }));
  }

  requestSubmit(event: SubmitEvent): void {
    event.preventDefault();
    if (!this.submitDisabled()) this.showSubmitConfirmation.set(true);
  }

  cancelSubmit(): void { this.showSubmitConfirmation.set(false); }

  confirmSubmit(): void { this.submit(false); }

  submit(isAutomatic = false): void {
    const assessment = this.assessment();
    if (!assessment || this.isSubmitting() || (!isAutomatic && !this.allAnswered())) return;
    this.showSubmitConfirmation.set(false);
    this.autoSubmitStarted = true;
    this.isSubmitting.set(true);
    this.errorMessage.set('');
    this.api.submitAssessment(this.assessmentId, {
      studentAnswers: assessment.questions
        .filter(question => Boolean(this.answers()[question.id]))
        .map(question => ({
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
    this.assessment.set(null);
    this.answers.set({});
    this.errorMessage.set('');
    this.autoSubmitStarted = false;
    this.load();
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
        next: response => {
          this.assessment.set(response.data);
          this.autoSubmitStarted = false;
          this.updateTimer();
        },
        error: (error: unknown) => this.errorMessage.set(normalizeApiError(error).message),
      });
  }

  private updateTimer(): void {
    const assessment = this.assessment();
    if (!assessment || this.result()) return;

    const expiresAt = Date.parse(assessment.attemptExpiresAtUtc);
    const remaining = Number.isNaN(expiresAt)
      ? assessment.durationMinutes * 60
      : Math.max(0, Math.ceil((expiresAt - Date.now()) / 1000));
    this.remainingSeconds.set(remaining);

    if (remaining === 0 && !this.autoSubmitStarted && !this.isSubmitting())
      this.submit(true);
  }
}
