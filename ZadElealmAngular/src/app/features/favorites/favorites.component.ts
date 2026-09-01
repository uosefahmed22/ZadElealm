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
import { CourseDto } from '../../core/catalog/catalog.models';
import { LearningApiService } from '../../core/learning/learning-api.service';
import { CourseCardComponent } from '../../shared/components/course-card/course-card.component';

@Component({
  selector: 'app-favorites',
  imports: [RouterLink, CourseCardComponent],
  templateUrl: './favorites.component.html',
  styleUrl: './favorites.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FavoritesComponent implements OnInit {
  private readonly learningApi = inject(LearningApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly courses = signal<readonly CourseDto[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly statusMessage = signal('');
  readonly pendingIds = signal<ReadonlySet<number>>(new Set());

  ngOnInit(): void {
    this.loadFavorites();
  }

  retry(): void {
    this.loadFavorites();
  }

  remove(course: CourseDto): void {
    if (this.pendingIds().has(course.id)) return;
    this.setPending(course.id, true);
    this.statusMessage.set('');
    this.learningApi
      .removeFavorite(course.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.courses.update((courses) => courses.filter((item) => item.id !== course.id));
          this.setPending(course.id, false);
          this.statusMessage.set(`تم حذف ${course.name} من المفضلة.`);
        },
        error: (error: unknown) => {
          this.setPending(course.id, false);
          this.statusMessage.set(normalizeApiError(error).message);
        },
      });
  }

  private loadFavorites(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.learningApi
      .getFavorites()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.courses.set(response.data.courses);
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.isLoading.set(false);
          this.errorMessage.set(normalizeApiError(error).message);
        },
      });
  }

  private setPending(courseId: number, pending: boolean): void {
    const nextIds = new Set(this.pendingIds());
    pending ? nextIds.add(courseId) : nextIds.delete(courseId);
    this.pendingIds.set(nextIds);
  }
}
