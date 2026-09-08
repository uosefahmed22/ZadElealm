import { CommonModule } from '@angular/common';
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
import { Subscription, take, timer } from 'rxjs';
import { normalizeApiError } from '../../core/api/api-error.utils';
import { CourseDto } from '../../core/catalog/catalog.models';
import { LearningApiService } from '../../core/learning/learning-api.service';
import { CourseProgressDto } from '../../core/learning/learning.models';
import { ArabicNumberPipe } from '../../shared/pipes/arabic-number.pipe';

interface EnrolledCourseView {
  course: CourseDto;
  progress: CourseProgressDto | null;
}

@Component({
  selector: 'app-my-courses',
  imports: [CommonModule, RouterLink, ArabicNumberPipe],
  templateUrl: './my-courses.component.html',
  styleUrl: './my-courses.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyCoursesComponent implements OnInit {
  private readonly learningApi = inject(LearningApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly courses = signal<readonly EnrolledCourseView[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly statusMessage = signal('');
  readonly confirmingCourseId = signal<number | null>(null);
  readonly pendingCourseId = signal<number | null>(null);
  readonly undoItem = signal<EnrolledCourseView | null>(null);
  readonly undoSeconds = signal(0);
  readonly restoringCourseId = signal<number | null>(null);
  private undoSubscription?: Subscription;

  ngOnInit(): void {
    this.loadCourses();
  }

  retry(): void {
    this.loadCourses();
  }

  requestUnenroll(courseId: number): void {
    this.confirmingCourseId.set(courseId);
    this.statusMessage.set('');
  }

  cancelUnenroll(): void {
    this.confirmingCourseId.set(null);
  }

  courseCategoryName(course: CourseDto): string {
    return course.category?.name || 'دورة تعليمية';
  }

  confirmUnenroll(item: EnrolledCourseView): void {
    if (this.pendingCourseId() !== null) return;

    this.pendingCourseId.set(item.course.id);
    this.statusMessage.set('');
    this.learningApi
      .unenroll(item.course.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.courses.update((courses) =>
            courses.filter((candidate) => candidate.course.id !== item.course.id),
          );
          this.confirmingCourseId.set(null);
          this.pendingCourseId.set(null);
          this.statusMessage.set(`تم إلغاء التسجيل في ${item.course.name}.`);
          this.startUndoWindow(item);
        },
        error: (error: unknown) => {
          this.pendingCourseId.set(null);
          this.statusMessage.set(normalizeApiError(error).message);
        },
      });
  }

  undoUnenroll(): void {
    const item = this.undoItem();
    if (!item || this.restoringCourseId() !== null) return;

    this.undoSubscription?.unsubscribe();
    this.restoringCourseId.set(item.course.id);
    this.learningApi
      .enroll(item.course.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.courses.update((courses) =>
            courses.some(candidate => candidate.course.id === item.course.id)
              ? courses
              : [item, ...courses],
          );
          this.undoItem.set(null);
          this.undoSeconds.set(0);
          this.restoringCourseId.set(null);
          this.statusMessage.set(`تمت استعادة ${item.course.name} مع الاحتفاظ بتقدمك.`);
        },
        error: (error: unknown) => {
          this.restoringCourseId.set(null);
          this.undoItem.set(null);
          this.undoSeconds.set(0);
          this.statusMessage.set(normalizeApiError(error).message);
        },
      });
  }

  private loadCourses(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.learningApi
      .getEnrolledCourses()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          const progressByCourse = new Map(
            (response.data.progress ?? []).map((item) => [item.courseId, item]),
          );
          this.courses.set(
            response.data.courses.map((course) => ({
              course,
              progress: progressByCourse.get(course.id) ?? null,
            })),
          );
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.isLoading.set(false);
          this.errorMessage.set(normalizeApiError(error).message);
        },
      });
  }

  private startUndoWindow(item: EnrolledCourseView): void {
    this.undoSubscription?.unsubscribe();
    this.undoItem.set(item);
    this.undoSeconds.set(10);
    this.undoSubscription = timer(1000, 1000)
      .pipe(take(10), takeUntilDestroyed(this.destroyRef))
      .subscribe(index => {
        const remaining = 9 - index;
        this.undoSeconds.set(remaining);
        if (remaining === 0) this.undoItem.set(null);
      });
  }
}
