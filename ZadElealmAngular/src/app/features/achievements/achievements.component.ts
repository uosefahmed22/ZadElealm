import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AchievementItem } from '../../core/achievements/achievement.models';
import { AchievementStateService } from '../../core/achievements/achievement-state.service';
import { AchievementBadgeComponent } from '../../shared/components/achievement-badge/achievement-badge.component';
import { ArabicNumberPipe } from '../../shared/pipes/arabic-number.pipe';

@Component({
  selector: 'app-achievements',
  imports: [AchievementBadgeComponent, ArabicNumberPipe],
  templateUrl: './achievements.component.html',
  styleUrl: './achievements.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AchievementsComponent implements OnInit {
  private readonly achievementState = inject(AchievementStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly dashboard = this.achievementState.dashboard;
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.loadAchievements(false);
  }

  retry(): void {
    this.loadAchievements(true);
  }

  isNew(achievement: AchievementItem): boolean {
    return this.dashboard()?.newlyUnlocked.includes(achievement.code) ?? false;
  }

  private loadAchievements(forceReload: boolean): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    const request = forceReload ? this.achievementState.reload() : this.achievementState.load();
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.isLoading.set(false),
      error: (error: unknown) => {
        this.isLoading.set(false);
        this.errorMessage.set(this.achievementState.errorMessage());
      },
    });
  }
}
