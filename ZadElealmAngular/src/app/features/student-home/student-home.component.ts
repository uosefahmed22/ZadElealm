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
import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { CertificateDto } from '../../core/assessments/assessment.models';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import { CatalogApiService } from '../../core/catalog/catalog-api.service';
import { CourseDto } from '../../core/catalog/catalog.models';
import { LearningApiService } from '../../core/learning/learning-api.service';
import { CourseProgressDto } from '../../core/learning/learning.models';
import { CourseCardComponent } from '../../shared/components/course-card/course-card.component';

interface DashboardCourse {
  course: CourseDto;
  progress: CourseProgressDto | null;
}

@Component({
  selector: 'app-student-home',
  imports: [CommonModule, DatePipe, RouterLink, CourseCardComponent],
  templateUrl: './student-home.component.html',
  styleUrl: './student-home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudentHomeComponent implements OnInit {
  readonly session = inject(AuthSessionService);
  private readonly learningApi = inject(LearningApiService);
  private readonly assessmentApi = inject(AssessmentApiService);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly enrolledCourses = signal<readonly DashboardCourse[]>([]);
  readonly visibleEnrolledCourses = computed(() => this.enrolledCourses().slice(0, 3));
  readonly certificates = signal<readonly CertificateDto[]>([]);
  readonly suggestedCourses = signal<readonly CourseDto[]>([]);
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

  ngOnInit(): void {
    this.loadDashboard();
  }
  retry(): void {
    this.loadDashboard();
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
      suggested: this.catalogApi.getCourses({
        categoryId: 0,
        search: '',
        author: '',
        language: '',
        minRating: 0,
        sortBy: 'date',
        sortDirection: 'desc',
        pageNumber: 1,
        pageSize: 3,
      }),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ enrolled, certificates, suggested }) => {
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
          const enrolledIds = new Set(courses.map((course) => course.id));
          this.suggestedCourses.set(
            suggested.data.filter((course) => !enrolledIds.has(course.id)).slice(0, 3),
          );
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.isLoading.set(false);
          this.errorMessage.set(normalizeApiError(error).message);
        },
      });
  }
}
