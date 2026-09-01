import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiDataResponseEnvelope } from '../api/api-error.models';
import { appEnvironment } from '../config/app-environment';
import { CertificateDto, QuizDto, QuizResultDto, QuizSubmissionDto } from './assessment.models';

@Injectable({ providedIn: 'root' })
export class AssessmentApiService {
  private readonly http = inject(HttpClient);
  private readonly api = appEnvironment.apiBaseUrl;

  getQuiz(quizId: number): Observable<ApiDataResponseEnvelope<QuizDto>> {
    return this.http.get<ApiDataResponseEnvelope<QuizDto>>(`${this.api}/Quiz/${quizId}`);
  }

  submitQuiz(payload: QuizSubmissionDto): Observable<ApiDataResponseEnvelope<QuizResultDto>> {
    return this.http.post<ApiDataResponseEnvelope<QuizResultDto>>(`${this.api}/Quiz/submit`, payload);
  }

  getCertificates(): Observable<ApiDataResponseEnvelope<CertificateDto[]>> {
    return this.http.get<ApiDataResponseEnvelope<CertificateDto[]>>(`${this.api}/Certificate/user`);
  }
}
