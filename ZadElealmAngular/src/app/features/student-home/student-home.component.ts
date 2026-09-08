import { CommonModule, DatePipe } from '@angular/common';
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
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { AchievementItem } from '../../core/achievements/achievement.models';
import { AchievementStateService } from '../../core/achievements/achievement-state.service';
import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { CertificateDto } from '../../core/assessments/assessment.models';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import { CourseDto } from '../../core/catalog/catalog.models';
import { LearningApiService } from '../../core/learning/learning-api.service';
import { CourseProgressDto } from '../../core/learning/learning.models';
import { AchievementBadgeComponent } from '../../shared/components/achievement-badge/achievement-badge.component';
import { CourseCardComponent } from '../../shared/components/course-card/course-card.component';
import { ArabicNumberPipe } from '../../shared/pipes/arabic-number.pipe';

interface DashboardCourse {
  course: CourseDto;
  progress: CourseProgressDto | null;
}

@Component({
  selector: 'app-student-home',
  imports: [
    CommonModule,
    DatePipe,
    RouterLink,
    AchievementBadgeComponent,
    CourseCardComponent,
    ArabicNumberPipe,
  ],
  templateUrl: './student-home.component.html',
  styleUrl: './student-home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudentHomeComponent implements OnInit {
  readonly session = inject(AuthSessionService);
  private readonly learningApi = inject(LearningApiService);
  private readonly assessmentApi = inject(AssessmentApiService);
  readonly achievementState = inject(AchievementStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly enrolledCourses = signal<readonly DashboardCourse[]>([]);
  readonly visibleEnrolledCourses = computed(() => this.enrolledCourses().slice(0, 3));
  readonly certificates = signal<readonly CertificateDto[]>([]);
  readonly favoriteCourses = signal<readonly CourseDto[]>([]);
  readonly achievementDashboard = this.achievementState.dashboard;
  readonly achievementError = this.achievementState.errorMessage;
  readonly visibleAchievements = computed(() =>
    this.sortAchievements(this.achievementDashboard()?.achievements ?? []).slice(0, 6),
  );
  readonly failedCourseImages = signal<ReadonlySet<number>>(new Set());
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly ongoingCount = computed(
    () =>
      this.enrolledCourses().filter((item) => (item.progress?.overallProgress ?? 0) < 100).length,
  );
  readonly completedCount = computed(
    () =>
      this.enrolledCourses().filter((item) => (item.progress?.overallProgress ?? 0) >= 100).length,
  );
  readonly closestToCompletionCourseId = computed(() => {
    const closestCourse = this.enrolledCourses()
      .filter((item) => {
        const progress = item.progress?.overallProgress ?? 0;
        return progress > 0 && progress < 100;
      })
      .reduce<DashboardCourse | null>((closest, item) => {
        if (!closest) {
          return item;
        }

        return (item.progress?.overallProgress ?? 0) > (closest.progress?.overallProgress ?? 0)
          ? item
          : closest;
      }, null);

    return closestCourse?.course.id ?? null;
  });

  ngOnInit(): void {
    this.loadDashboard();
  }
  retry(): void {
    this.loadDashboard();
  }

  retryAchievements(): void {
    this.achievementState
      .reload()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ error: () => undefined });
  }

  isNewAchievement(achievement: AchievementItem): boolean {
    return this.achievementDashboard()?.newlyUnlocked.includes(achievement.code) ?? false;
  }

  courseCategoryName(course: CourseDto): string {
    return course.category?.name || 'دورة تعليمية';
  }

  courseImage(course: CourseDto): string {
    return !course.imageUrl || this.failedCourseImages().has(course.id)
      ? 'assets/brand/course-placeholder.svg'
      : course.imageUrl;
  }

  markCourseImageFailed(courseId: number): void {
    this.failedCourseImages.update((ids) => new Set(ids).add(courseId));
  }

  private loadDashboard(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    forkJoin({
      enrolled: this.learningApi.getEnrolledCourses(),
      certificates: this.assessmentApi.getCertificates(),
      favorites: this.learningApi.getFavorites(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ enrolled, certificates, favorites }) => {
          const courses = enrolled.data.courses;
          const progressByCourse = new Map(
            (enrolled.data.progress ?? []).map((item) => [item.courseId, item]),
          );

          this.enrolledCourses.set(
            courses.map((course) => ({
              course,
              progress: progressByCourse.get(course.id) ?? null,
            })),
          );
          this.certificates.set(certificates.data);
          this.favoriteCourses.set(favorites.data.courses.slice(0, 3));
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.isLoading.set(false);
          this.errorMessage.set(normalizeApiError(error).message);
        },
      });
  }

  private sortAchievements(achievements: readonly AchievementItem[]): AchievementItem[] {
    const newlyUnlocked = new Set(this.achievementDashboard()?.newlyUnlocked ?? []);
    return [...achievements].sort((left, right) => {
      const newDifference =
        Number(newlyUnlocked.has(right.code)) - Number(newlyUnlocked.has(left.code));
      if (newDifference !== 0) return newDifference;

      const unlockedDifference = Number(right.isUnlocked) - Number(left.isUnlocked);
      if (unlockedDifference !== 0) return unlockedDifference;

      if (left.isUnlocked && right.isUnlocked) {
        return (right.unlockedAtUtc ?? '').localeCompare(left.unlockedAtUtc ?? '');
      }

      const leftProgress = left.target > 0 ? left.currentValue / left.target : 0;
      const rightProgress = right.target > 0 ? right.currentValue / right.target : 0;
      return rightProgress - leftProgress;
    });
  }
}
