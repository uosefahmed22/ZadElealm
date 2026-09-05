import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiDataResponseEnvelope } from '../api/api-error.models';
import { appEnvironment } from '../config/app-environment';
import {
  AssessmentResultDto,
  AssessmentSubmissionDto,
  AssessmentSummaryDto,
  CategoryAssessmentDto,
  CertificateDto,
  QuizDto,
  QuizResultDto,
  QuizSubmissionDto,
} from './assessment.models';

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

  downloadCertificate(certificateId: number): Observable<Blob> {
    return this.http.get(`${this.api}/Certificate/${certificateId}/file`, {
      responseType: 'blob',
    });
  }

  getAssessments(): Observable<ApiDataResponseEnvelope<AssessmentSummaryDto[]>> {
    return this.http.get<ApiDataResponseEnvelope<AssessmentSummaryDto[]>>(`${this.api}/Assessment`);
  }

  getAssessment(assessmentId: number): Observable<ApiDataResponseEnvelope<CategoryAssessmentDto>> {
    return this.http.get<ApiDataResponseEnvelope<CategoryAssessmentDto>>(
      `${this.api}/Assessment/${assessmentId}`,
    );
  }

  submitAssessment(
    assessmentId: number,
    payload: AssessmentSubmissionDto,
  ): Observable<ApiDataResponseEnvelope<AssessmentResultDto>> {
    return this.http.post<ApiDataResponseEnvelope<AssessmentResultDto>>(
      `${this.api}/Assessment/${assessmentId}/submit`,
      payload,
    );
  }
}
