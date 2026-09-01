export type RankTier = 'Bronze' | 'Silver' | 'Gold' | 'Platinum' | 'Diamond';

export interface StudentRankSummary {
  totalPoints: number;
  rank: RankTier;
  completedCoursesCount: number;
  certificatesCount: number;
  averageQuizScore: number;
  lastUpdated: string;
}

export interface LeaderboardEntry {
  position: number;
  displayName: string;
  imageUrl: string | null;
  totalPoints: number;
  rank: RankTier;
  completedCoursesCount: number;
  isCurrentUser: boolean;
}

export interface RankDashboard {
  currentUser: StudentRankSummary;
  leaders: LeaderboardEntry[];
}
