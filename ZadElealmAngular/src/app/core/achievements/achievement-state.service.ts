import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, map, of, shareReplay, tap, throwError } from 'rxjs';

import { normalizeApiError } from '../api/api-error.utils';
import { AchievementApiService } from './achievement-api.service';
import { AchievementDashboard } from './achievement.models';

@Injectable({ providedIn: 'root' })
export class AchievementStateService {
  private readonly api = inject(AchievementApiService);
  private request: Observable<AchievementDashboard> | null = null;

  readonly dashboard = signal<AchievementDashboard | null>(null);
  readonly isLoading = signal(false);
  readonly errorMessage = signal('');
  readonly currentStreak = computed(() => this.dashboard()?.currentStreak ?? 0);

  load(): Observable<AchievementDashboard> {
    const current = this.dashboard();
    return current ? of(current) : this.executeCheckIn();
  }

  reload(): Observable<AchievementDashboard> {
    return this.executeCheckIn();
  }

  private executeCheckIn(): Observable<AchievementDashboard> {
    if (this.request) return this.request;

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.request = this.api.checkIn().pipe(
      map((response) => response.data),
      tap((dashboard) => {
        this.dashboard.set(dashboard);
        this.isLoading.set(false);
      }),
      catchError((error: unknown) => {
        this.isLoading.set(false);
        this.errorMessage.set(normalizeApiError(error).message);
        return throwError(() => error);
      }),
      finalize(() => (this.request = null)),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    return this.request;
  }
}
