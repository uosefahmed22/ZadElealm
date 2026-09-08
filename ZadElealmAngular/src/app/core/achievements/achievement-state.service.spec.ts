import { TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';

import { ApiDataResponseEnvelope } from '../api/api-error.models';
import { AchievementApiService } from './achievement-api.service';
import { AchievementDashboard } from './achievement.models';
import { AchievementStateService } from './achievement-state.service';

describe('AchievementStateService', () => {
  it('shares one check-in request between navbar and page consumers', () => {
    const response = new Subject<ApiDataResponseEnvelope<AchievementDashboard>>();
    const api = { checkIn: vi.fn(() => response.asObservable()) };
    TestBed.configureTestingModule({
      providers: [AchievementStateService, { provide: AchievementApiService, useValue: api }],
    });
    const state = TestBed.inject(AchievementStateService);

    state.load().subscribe();
    state.load().subscribe();
    expect(api.checkIn).toHaveBeenCalledOnce();

    response.next({ statusCode: 200, data: dashboard() });
    response.complete();
    expect(state.currentStreak()).toBe(3);
  });
});

function dashboard(): AchievementDashboard {
  return {
    currentStreak: 3,
    longestStreak: 5,
    unlockedCount: 1,
    totalCount: 11,
    newlyUnlocked: [],
    achievements: [],
  };
}
