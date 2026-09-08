import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { RankApiService } from '../../core/rank/rank-api.service';
import { LeaderboardComponent } from './leaderboard.component';

describe('LeaderboardComponent', () => {
  it('renders the current student summary and real ordered leaders', () => {
    TestBed.configureTestingModule({
      imports: [LeaderboardComponent],
      providers: [
        { provide: RankApiService, useValue: { getDashboard: () => of(rankResponse()) } },
      ],
    });

    const fixture = TestBed.createComponent(LeaderboardComponent);
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('ذهبي');
    expect(text).toContain('٤٢٠ نقطة');
    expect(text).toContain('سارة علي');
    expect(text).toContain('أنت');
    expect(text).toContain('٣٣ دورة × ١٠ نقاط');
    expect(text).toContain('٢ شهادة × ٢٠ نقطة');
    expect(fixture.nativeElement.querySelectorAll('.podium-card')).toHaveLength(3);
    expect(fixture.nativeElement.querySelectorAll('.leader-row')).toHaveLength(2);
  });

  it('shows the API error and retries without inventing leaderboard rows', () => {
    const getDashboard = vi
      .fn()
      .mockReturnValueOnce(throwError(() => new Error('تعذر الاتصال')))
      .mockReturnValueOnce(of(rankResponse()));
    TestBed.configureTestingModule({
      imports: [LeaderboardComponent],
      providers: [{ provide: RankApiService, useValue: { getDashboard } }],
    });

    const fixture = TestBed.createComponent(LeaderboardComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('تعذر الاتصال');
    expect(fixture.nativeElement.querySelectorAll('.podium-card')).toHaveLength(0);

    (fixture.nativeElement.querySelector('.state-card button') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(getDashboard).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('سارة علي');
  });
});

function rankResponse() {
  return {
    statusCode: 200,
    data: {
      currentUser: {
        totalPoints: 420,
        rank: 'Gold' as const,
        completedCoursesCount: 33,
        certificatesCount: 2,
        averageQuizScore: 100,
        lastUpdated: '2026-09-01T10:00:00Z',
        pointsBreakdown: {
          completedCoursesPoints: 330,
          certificatesPoints: 40,
          quizAverageBonusPoints: 50,
          pointsPerCompletedCourse: 10,
          pointsPerCertificate: 20,
          quizAverageContributionPercentage: 50,
        },
      },
      leaders: [
        {
          position: 1,
          displayName: 'سارة علي',
          imageUrl: null,
          totalPoints: 650,
          rank: 'Platinum' as const,
          completedCoursesCount: 10,
          isCurrentUser: false,
        },
        {
          position: 2,
          displayName: 'يوسف أحمد',
          imageUrl: null,
          totalPoints: 420,
          rank: 'Gold' as const,
          completedCoursesCount: 33,
          isCurrentUser: true,
        },
        {
          position: 3,
          displayName: 'أحمد حسن',
          imageUrl: null,
          totalPoints: 390,
          rank: 'Gold' as const,
          completedCoursesCount: 6,
          isCurrentUser: false,
        },
        {
          position: 4,
          displayName: 'مريم محمد',
          imageUrl: null,
          totalPoints: 320,
          rank: 'Gold' as const,
          completedCoursesCount: 5,
          isCurrentUser: false,
        },
        {
          position: 5,
          displayName: 'عمر خالد',
          imageUrl: null,
          totalPoints: 290,
          rank: 'Silver' as const,
          completedCoursesCount: 4,
          isCurrentUser: false,
        },
      ],
      tiers: [
        { rank: 'Bronze' as const, minimumPoints: 0, maximumPoints: 99 },
        { rank: 'Silver' as const, minimumPoints: 100, maximumPoints: 299 },
        { rank: 'Gold' as const, minimumPoints: 300, maximumPoints: 599 },
        { rank: 'Platinum' as const, minimumPoints: 600, maximumPoints: 999 },
        { rank: 'Diamond' as const, minimumPoints: 1000, maximumPoints: null },
      ],
    },
  };
}
