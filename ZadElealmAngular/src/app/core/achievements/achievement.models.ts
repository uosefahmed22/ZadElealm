export interface AchievementDashboard {
  currentStreak: number;
  longestStreak: number;
  unlockedCount: number;
  totalCount: number;
  newlyUnlocked: string[];
  achievements: AchievementItem[];
}

export interface AchievementItem {
  code: string;
  title: string;
  description: string;
  iconKey: string;
  isUnlocked: boolean;
  unlockedAtUtc: string | null;
  currentValue: number;
  target: number;
}
