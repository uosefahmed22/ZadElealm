import { DatePipe, DecimalPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { RankApiService } from '../../core/rank/rank-api.service';
import { LeaderboardEntry, RankDashboard, RankTier } from '../../core/rank/rank.models';

const rankLabels: Record<RankTier, string> = {
  Bronze: 'برونزي',
  Silver: 'فضي',
  Gold: 'ذهبي',
  Platinum: 'بلاتيني',
  Diamond: 'ماسي',
};

const nextThresholds: Record<RankTier, number | null> = {
  Bronze: 100,
  Silver: 300,
  Gold: 600,
  Platinum: 1000,
  Diamond: null,
};

const rankStarts: Record<RankTier, number> = {
  Bronze: 0,
  Silver: 100,
  Gold: 300,
  Platinum: 600,
  Diamond: 1000,
};

@Component({
  selector: 'app-leaderboard',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LeaderboardComponent implements OnInit {
  private readonly rankApi = inject(RankApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly dashboard = signal<RankDashboard | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly brokenImages = signal<readonly number[]>([]);

  ngOnInit(): void {
    this.loadDashboard();
  }

  retry(): void {
    this.loadDashboard();
  }

  rankLabel(rank: RankTier): string {
    return rankLabels[rank];
  }

  pointsToNext(rank: RankTier, points: number): number | null {
    const threshold = nextThresholds[rank];
    return threshold === null ? null : Math.max(0, threshold - points);
  }

  progressPercentage(rank: RankTier, points: number): number {
    const threshold = nextThresholds[rank];
    if (threshold === null) return 100;
    const start = rankStarts[rank];
    return Math.min(100, Math.max(0, ((points - start) / (threshold - start)) * 100));
  }

  initials(entry: LeaderboardEntry): string {
    return entry.displayName
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0])
      .join('');
  }

  hideBrokenImage(position: number): void {
    this.brokenImages.update((positions) => [...positions, position]);
  }

  imageIsAvailable(entry: LeaderboardEntry): boolean {
    return Boolean(entry.imageUrl) && !this.brokenImages().includes(entry.position);
  }

  private loadDashboard(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.rankApi
      .getDashboard()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.dashboard.set(response.data);
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(normalizeApiError(error).message);
          this.isLoading.set(false);
        },
      });
  }
}
