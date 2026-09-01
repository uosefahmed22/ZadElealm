import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  Subject,
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  of,
  startWith,
  switchMap,
} from 'rxjs';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { CatalogApiService } from '../../core/catalog/catalog-api.service';
import {
  CategoryDto,
  CourseCatalogFilters,
  CourseDto,
  PaginatedCoursesResponse,
  PaginationMetadata,
} from '../../core/catalog/catalog.models';
import { CourseCardComponent } from '../../shared/components/course-card/course-card.component';
import { LearningApiService } from '../../core/learning/learning-api.service';

type CatalogLoadResult =
  { response: PaginatedCoursesResponse; error: '' } | { response: null; error: string };

@Component({
  selector: 'app-course-catalog',
  imports: [CommonModule, ReactiveFormsModule, CourseCardComponent],
  templateUrl: './course-catalog.component.html',
  styleUrl: './course-catalog.component.scss',
})
export class CourseCatalogComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly learningApi = inject(LearningApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly reloadCourses$ = new Subject<void>();

  readonly categories = signal<CategoryDto[]>([]);
  readonly courses = signal<CourseDto[]>([]);
  readonly metadata = signal<PaginationMetadata | null>(null);
  readonly isLoading = signal(true);
  readonly categoriesError = signal('');
  readonly errorMessage = signal('');
  readonly pageNumber = signal(1);
  readonly favoriteIds = signal<ReadonlySet<number>>(new Set());
  readonly pendingFavoriteIds = signal<ReadonlySet<number>>(new Set());
  readonly favoriteMessage = signal('');
  readonly skeletonCards = Array.from({ length: 6 });

  readonly filterForm = this.formBuilder.nonNullable.group({
    search: '',
    categoryId: 0,
    author: '',
    language: '',
    minRating: 0,
    sort: 'date-desc',
  });

  readonly resultSummary = computed(() => {
    const total = this.metadata()?.totalMatchedItems ?? 0;
    if (this.isLoading()) {
      return 'جارٍ تحميل الدورات…';
    }
    const categoryCount = this.categories().length;
    const courseLabel = total === 1 ? 'دورة واحدة' : `${total} دورة`;
    const categoryLabel = categoryCount === 1 ? 'تصنيف واحد' : `${categoryCount} تصنيفًا`;
    return `${courseLabel} في ${categoryLabel}`;
  });

  readonly visiblePages = computed(() => {
    const metadata = this.metadata();
    if (!metadata || metadata.numberOfPages <= 1) {
      return [];
    }

    const start = Math.max(1, metadata.currentPage - 2);
    const end = Math.min(metadata.numberOfPages, metadata.currentPage + 2);
    return Array.from({ length: end - start + 1 }, (_, index) => start + index);
  });

  ngOnInit(): void {
    this.loadCategories();
    this.loadFavorites();
    this.reloadCourses$
      .pipe(
        startWith(undefined),
        switchMap(() => {
          this.isLoading.set(true);
          this.errorMessage.set('');
          return this.catalogApi.getCourses(this.buildFilters()).pipe(
            map((response): CatalogLoadResult => ({
              response,
              error: '',
            })),
            catchError((error: unknown) =>
              of<CatalogLoadResult>({
                response: null,
                error: normalizeApiError(error).message,
              }),
            ),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((result) => {
        this.isLoading.set(false);
        if (!result.response) {
          this.courses.set([]);
          this.metadata.set(null);
          this.errorMessage.set(result.error);
          return;
        }

        this.courses.set(result.response.data);
        this.metadata.set(result.response.metaData);
        this.pageNumber.set(result.response.metaData.currentPage);
      });

    this.filterForm.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.applyFilters());
  }

  selectCategory(categoryId: number): void {
    if (this.filterForm.controls.categoryId.value === categoryId) return;
    this.filterForm.controls.categoryId.setValue(categoryId);
    this.applyFilters();
  }

  applyFilters(): void {
    this.pageNumber.set(1);
    this.reloadCourses$.next();
  }

  resetFilters(): void {
    this.filterForm.reset({
      search: '',
      categoryId: 0,
      author: '',
      language: '',
      minRating: 0,
      sort: 'date-desc',
    });
    this.applyFilters();
  }

  changePage(page: number): void {
    const totalPages = this.metadata()?.numberOfPages ?? 1;
    if (page < 1 || page > totalPages || page === this.pageNumber()) {
      return;
    }

    this.pageNumber.set(page);
    this.reloadCourses$.next();
  }

  retry(): void {
    this.reloadCourses$.next();
  }

  toggleFavorite(course: CourseDto): void {
    if (this.pendingFavoriteIds().has(course.id)) return;

    const isFavorite = this.favoriteIds().has(course.id);
    this.setFavoritePending(course.id, true);
    this.favoriteMessage.set('');
    const request = isFavorite
      ? this.learningApi.removeFavorite(course.id)
      : this.learningApi.addFavorite(course.id);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        const nextIds = new Set(this.favoriteIds());
        isFavorite ? nextIds.delete(course.id) : nextIds.add(course.id);
        this.favoriteIds.set(nextIds);
        this.setFavoritePending(course.id, false);
        this.favoriteMessage.set(
          isFavorite
            ? `تم حذف ${course.name} من المفضلة.`
            : `تمت إضافة ${course.name} إلى المفضلة.`,
        );
      },
      error: (error: unknown) => {
        this.setFavoritePending(course.id, false);
        this.favoriteMessage.set(normalizeApiError(error).message);
      },
    });
  }

  private loadCategories(): void {
    this.catalogApi
      .getCategories()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => this.categories.set(response.data),
        error: () =>
          this.categoriesError.set('تعذر تحميل التصنيفات، وما زال بإمكانك تصفح الدورات.'),
      });
  }

  private loadFavorites(): void {
    this.learningApi
      .getFavorites()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) =>
          this.favoriteIds.set(new Set(response.data.courses.map((course) => course.id))),
        error: () => this.favoriteMessage.set('تعذر تحديد الدورات المفضلة حاليًا.'),
      });
  }

  private setFavoritePending(courseId: number, pending: boolean): void {
    const nextIds = new Set(this.pendingFavoriteIds());
    pending ? nextIds.add(courseId) : nextIds.delete(courseId);
    this.pendingFavoriteIds.set(nextIds);
  }

  private buildFilters(): CourseCatalogFilters {
    const form = this.filterForm.getRawValue();
    const [sortBy, sortDirection] = form.sort.split('-') as [
      CourseCatalogFilters['sortBy'],
      CourseCatalogFilters['sortDirection'],
    ];

    return {
      categoryId: form.categoryId,
      search: form.search.trim(),
      author: form.author.trim(),
      language: form.language,
      minRating: form.minRating,
      sortBy,
      sortDirection,
      pageNumber: this.pageNumber(),
      pageSize: 9,
    };
  }
}
