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

import { normalizeApiError } from '../../core/api/api-error.utils';
import {
  formatArabicDateTime,
  formatArabicNumber,
} from '../../core/i18n/arabic-number-format.util';
import { RankApiService } from '../../core/rank/rank-api.service';
import {
  LeaderboardEntry,
  RankDashboard,
  RankTier,
  RankTierDefinition,
} from '../../core/rank/rank.models';
import { ArabicNumberPipe } from '../../shared/pipes/arabic-number.pipe';

const rankLabels: Record<RankTier, string> = {
  Bronze: 'برونزي',
  Silver: 'فضي',
  Gold: 'ذهبي',
  Platinum: 'بلاتيني',
  Diamond: 'ماسي',
};

@Component({
  selector: 'app-leaderboard',
  imports: [ArabicNumberPipe],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LeaderboardComponent implements OnInit {
  readonly formatArabicDateTime = formatArabicDateTime;
  private readonly rankApi = inject(RankApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly dashboard = signal<RankDashboard | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly brokenImages = signal<readonly number[]>([]);
  readonly podiumLeaders = computed(() => this.dashboard()?.leaders.slice(0, 3) ?? []);
  readonly remainingLeaders = computed(() => this.dashboard()?.leaders.slice(3, 10) ?? []);

  ngOnInit(): void {
    this.loadDashboard();
  }

  retry(): void {
    this.loadDashboard();
  }

  rankLabel(rank: RankTier): string {
    return rankLabels[rank];
  }

  pointsToNext(tiers: RankTierDefinition[], rank: RankTier, points: number): number | null {
    const tierIndex = tiers.findIndex((tier) => tier.rank === rank);
    const nextTier = tiers[tierIndex + 1];
    return nextTier ? Math.max(0, nextTier.minimumPoints - points) : null;
  }

  progressPercentage(tiers: RankTierDefinition[], rank: RankTier, points: number): number {
    const tierIndex = tiers.findIndex((tier) => tier.rank === rank);
    const currentTier = tiers[tierIndex];
    const nextTier = tiers[tierIndex + 1];
    if (!currentTier || !nextTier) return 100;
    return Math.min(
      100,
      Math.max(
        0,
        ((points - currentTier.minimumPoints) /
          (nextTier.minimumPoints - currentTier.minimumPoints)) *
          100,
      ),
    );
  }

  tierRange(tier: RankTierDefinition): string {
    return tier.maximumPoints === null
      ? `${formatArabicNumber(tier.minimumPoints)}+ نقطة`
      : `${formatArabicNumber(tier.minimumPoints)}–${formatArabicNumber(tier.maximumPoints)}`;
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
