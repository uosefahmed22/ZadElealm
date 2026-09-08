import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiDataResponseEnvelope } from '../api/api-error.models';
import { appEnvironment } from '../config/app-environment';
import { AchievementDashboard } from './achievement.models';

@Injectable({ providedIn: 'root' })
export class AchievementApiService {
  private readonly http = inject(HttpClient);

  getMine(): Observable<ApiDataResponseEnvelope<AchievementDashboard>> {
    return this.http.get<ApiDataResponseEnvelope<AchievementDashboard>>(
      `${appEnvironment.apiBaseUrl}/Achievements/mine`,
    );
  }

  checkIn(): Observable<ApiDataResponseEnvelope<AchievementDashboard>> {
    return this.http.post<ApiDataResponseEnvelope<AchievementDashboard>>(
      `${appEnvironment.apiBaseUrl}/Achievements/check-in`,
      {},
    );
  }
}
