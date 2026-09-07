export type RankTier = 'Bronze' | 'Silver' | 'Gold' | 'Platinum' | 'Diamond';

export interface StudentRankSummary {
  totalPoints: number;
  rank: RankTier;
  completedCoursesCount: number;
  certificatesCount: number;
  averageQuizScore: number;
  lastUpdated: string;
  pointsBreakdown: RankPointsBreakdown;
}

export interface RankPointsBreakdown {
  completedCoursesPoints: number;
  certificatesPoints: number;
  quizAverageBonusPoints: number;
  pointsPerCompletedCourse: number;
  pointsPerCertificate: number;
  quizAverageContributionPercentage: number;
}

export interface RankTierDefinition {
  rank: RankTier;
  minimumPoints: number;
  maximumPoints: number | null;
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
  tiers: RankTierDefinition[];
}
