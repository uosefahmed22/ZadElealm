import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { appEnvironment } from '../config/app-environment';
import { AchievementApiService } from './achievement-api.service';

describe('AchievementApiService', () => {
  let service: AchievementApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AchievementApiService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AchievementApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads the current user achievements', () => {
    service.getMine().subscribe();

    const request = http.expectOne(`${appEnvironment.apiBaseUrl}/Achievements/mine`);
    expect(request.request.method).toBe('GET');
    request.flush({ statusCode: 200, data: dashboard() });
  });

  it('checks in the current user without a fake payload', () => {
    service.checkIn().subscribe();

    const request = http.expectOne(`${appEnvironment.apiBaseUrl}/Achievements/check-in`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({});
    request.flush({ statusCode: 200, data: dashboard() });
  });
});

function dashboard() {
  return {
    currentStreak: 1,
    longestStreak: 1,
    unlockedCount: 0,
    totalCount: 11,
    newlyUnlocked: [],
    achievements: [],
  };
}
