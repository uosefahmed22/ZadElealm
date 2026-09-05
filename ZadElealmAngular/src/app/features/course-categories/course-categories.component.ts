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

import { normalizeApiError } from '../../core/api/api-error.utils';
import { CatalogApiService } from '../../core/catalog/catalog-api.service';
import { CategoryDto } from '../../core/catalog/catalog.models';

@Component({
  selector: 'app-course-categories',
  imports: [RouterLink],
  templateUrl: './course-categories.component.html',
  styleUrl: './course-categories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CourseCategoriesComponent implements OnInit {
  private readonly catalogApi = inject(CatalogApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly categories = signal<readonly CategoryDto[]>([]);
  readonly failedImageIds = signal<ReadonlySet<number>>(new Set());
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly skeletonCards = Array.from({ length: 6 });
  readonly fallbackImage = 'assets/brand/course-placeholder.svg';

  ngOnInit(): void {
    this.loadCategories();
  }

  retry(): void {
    this.loadCategories();
  }

  categoryImage(category: CategoryDto): string {
    if (!category.imageUrl || this.failedImageIds().has(category.id)) {
      return this.fallbackImage;
    }

    return category.imageUrl.replace(/^http:\/\//i, 'https://');
  }

  markImageFailed(categoryId: number): void {
    this.failedImageIds.update((ids) => new Set(ids).add(categoryId));
  }

  private loadCategories(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.catalogApi
      .getCategories()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.categories.set(response.data);
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(normalizeApiError(error).message);
          this.isLoading.set(false);
        },
      });
  }
}
