import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { AchievementItem } from '../../../core/achievements/achievement.models';
import { ArabicNumberPipe } from '../../pipes/arabic-number.pipe';

@Component({
  selector: 'app-achievement-badge',
  imports: [ArabicNumberPipe],
  templateUrl: './achievement-badge.component.html',
  styleUrl: './achievement-badge.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AchievementBadgeComponent {
  readonly achievement = input.required<AchievementItem>();
  readonly isNew = input(false);
  readonly streakDays = input<number | null>(null);

  isHotStreak(): boolean {
    return (this.streakDays() ?? this.achievement().currentValue) >= 10;
  }

  progressPercent(): number {
    const item = this.achievement();
    return item.target > 0 ? Math.min(100, Math.round((item.currentValue / item.target) * 100)) : 0;
  }
}
