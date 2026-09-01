import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiResponseEnvelope } from '../api/api-error.models';
import { appEnvironment } from '../config/app-environment';
import { CreateSupportReportRequest } from './support.models';

@Injectable({ providedIn: 'root' })
export class SupportApiService {
  private readonly http = inject(HttpClient);
  private readonly reportUrl = `${appEnvironment.apiBaseUrl}/Report`;

  createReport(payload: CreateSupportReportRequest): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(this.reportUrl, payload);
  }
}
