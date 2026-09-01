import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiDataResponseEnvelope } from '../api/api-error.models';
import { appEnvironment } from '../config/app-environment';
import { RankDashboard } from './rank.models';

@Injectable({ providedIn: 'root' })
export class RankApiService {
  private readonly http = inject(HttpClient);

  getDashboard(take = 10): Observable<ApiDataResponseEnvelope<RankDashboard>> {
    return this.http.get<ApiDataResponseEnvelope<RankDashboard>>(
      `${appEnvironment.apiBaseUrl}/Rank/dashboard`,
      { params: { take } },
    );
  }
}
