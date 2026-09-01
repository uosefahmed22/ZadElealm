import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { appEnvironment } from '../config/app-environment';
import { RankApiService } from './rank-api.service';

describe('RankApiService', () => {
  it('loads the protected dashboard contract with a bounded leaderboard size', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    const service = TestBed.inject(RankApiService);
    const http = TestBed.inject(HttpTestingController);

    service.getDashboard(10).subscribe();

    const request = http.expectOne(`${appEnvironment.apiBaseUrl}/Rank/dashboard?take=10`);
    expect(request.request.method).toBe('GET');
    request.flush({ statusCode: 200, data: { currentUser: {}, leaders: [] } });
    http.verify();
  });
});
