import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, map, of, startWith, switchMap } from 'rxjs';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { CatalogApiService } from '../../core/catalog/catalog-api.service';
import {
  CategoryDto,
  CourseCatalogFilters,
  CourseDto,
  PaginatedCoursesResponse,
  PaginationMetadata,
} from '../../core/catalog/catalog.models';

type CatalogLoadResult =
  { response: PaginatedCoursesResponse; error: '' } | { response: null; error: string };

@Component({
  selector: 'app-course-catalog',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './course-catalog.component.html',
  styleUrl: './course-catalog.component.scss',
})
export class CourseCatalogComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly reloadCourses$ = new Subject<void>();

  readonly categories = signal<CategoryDto[]>([]);
  readonly courses = signal<CourseDto[]>([]);
  readonly metadata = signal<PaginationMetadata | null>(null);
  readonly isLoading = signal(true);
  readonly categoriesError = signal('');
  readonly errorMessage = signal('');
  readonly pageNumber = signal(1);
  readonly failedImageIds = signal<ReadonlySet<number>>(new Set<number>());
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
    return total === 1 ? 'دورة واحدة' : `${total} دورة`;
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

  markImageFailed(courseId: number): void {
    this.failedImageIds.update((ids) => new Set(ids).add(courseId));
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
