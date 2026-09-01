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
    expect(text).toContain('420 نقطة');
    expect(text).toContain('سارة علي');
    expect(text).toContain('أنت');
    expect(fixture.nativeElement.querySelectorAll('.leader-list li')).toHaveLength(2);
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
    expect(fixture.nativeElement.querySelectorAll('.leader-list li')).toHaveLength(0);

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
        completedCoursesCount: 7,
        certificatesCount: 2,
        averageQuizScore: 86.5,
        lastUpdated: '2026-09-01T10:00:00Z',
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
          completedCoursesCount: 7,
          isCurrentUser: true,
        },
      ],
    },
  };
}
