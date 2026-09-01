import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { QuizDto, QuizResultDto } from '../../core/assessments/assessment.models';

@Component({
  selector: 'app-quiz',
  imports: [CommonModule, RouterLink],
  templateUrl: './quiz.component.html',
  styleUrl: './quiz.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuizComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AssessmentApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly courseId = Number(this.route.snapshot.paramMap.get('courseId'));
  readonly quizId = Number(this.route.snapshot.paramMap.get('quizId'));
  readonly quiz = signal<QuizDto | null>(null);
  readonly answers = signal<Readonly<Record<number, number>>>({});
  readonly result = signal<QuizResultDto | null>(null);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');
  readonly answeredCount = computed(() => Object.keys(this.answers()).length);
  readonly allAnswered = computed(() => {
    const total = this.quiz()?.questions.length ?? 0;
    return total > 0 && this.answeredCount() === total;
  });
  readonly submitDisabled = computed(() => !this.allAnswered() || this.isSubmitting());

  ngOnInit(): void {
    this.loadQuiz();
  }

  selectAnswer(questionId: number, choiceId: number): void {
    if (this.isSubmitting()) return;
    this.answers.update((answers) => ({ ...answers, [questionId]: choiceId }));
  }

  submit(): void {
    const quiz = this.quiz();
    if (!quiz || this.submitDisabled()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set('');
    this.api
      .submitQuiz({
        quizId: quiz.id,
        studentAnswers: quiz.questions.map((question) => ({
          questionId: question.id,
          choiceId: this.answers()[question.id],
        })),
      })
      .pipe(finalize(() => this.isSubmitting.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => this.result.set(response.data),
        error: (error: unknown) => this.errorMessage.set(normalizeApiError(error).message),
      });
  }

  retryQuiz(): void {
    this.result.set(null);
    this.answers.set({});
    this.errorMessage.set('');
  }

  retryLoad(): void {
    this.loadQuiz();
  }

  private loadQuiz(): void {
    if (!Number.isInteger(this.quizId) || this.quizId <= 0) {
      this.isLoading.set(false);
      this.errorMessage.set('رابط الاختبار غير صالح.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.api
      .getQuiz(this.quizId)
      .pipe(finalize(() => this.isLoading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => this.quiz.set(response.data),
        error: (error: unknown) => this.errorMessage.set(normalizeApiError(error).message),
      });
  }
}
