import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiDataResponseEnvelope, ApiResponseEnvelope } from '../api/api-error.models';
import { appEnvironment } from '../config/app-environment';
import {
  CourseDetailsDto,
  CourseProgressDto,
  CourseReplyDto,
  EligibilityResponse,
  EnrolledCoursesData,
  FavoriteCoursesData,
  VideoProgressDto,
} from './learning.models';

@Injectable({ providedIn: 'root' })
export class LearningApiService {
  private readonly http = inject(HttpClient);
  private readonly api = appEnvironment.apiBaseUrl;

  getCourse(courseId: number): Observable<ApiDataResponseEnvelope<CourseDetailsDto>> {
    return this.http.get<ApiDataResponseEnvelope<CourseDetailsDto>>(
      `${this.api}/Course/${courseId}`,
    );
  }

  getCourseProgress(courseId: number): Observable<CourseProgressDto> {
    return this.http.get<CourseProgressDto>(`${this.api}/VideoProgress/course/${courseId}`);
  }

  checkEligibility(courseId: number): Observable<EligibilityResponse> {
    return this.http.get<EligibilityResponse>(
      `${this.api}/VideoProgress/check-eligibility/${courseId}`,
    );
  }

  updateProgress(
    videoId: number,
    watchedSeconds: number,
  ): Observable<ApiDataResponseEnvelope<VideoProgressDto>> {
    return this.http.post<ApiDataResponseEnvelope<VideoProgressDto>>(
      `${this.api}/VideoProgress/update`,
      { videoId, watchedSeconds },
    );
  }

  enroll(courseId: number): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.api}/Enrollment/${courseId}`, null);
  }

  getEnrolledCourses(): Observable<ApiDataResponseEnvelope<EnrolledCoursesData>> {
    return this.http.get<ApiDataResponseEnvelope<EnrolledCoursesData>>(`${this.api}/Enrollment`);
  }

  unenroll(courseId: number): Observable<ApiResponseEnvelope> {
    return this.http.delete<ApiResponseEnvelope>(`${this.api}/Enrollment/${courseId}`);
  }

  getFavorites(): Observable<ApiDataResponseEnvelope<FavoriteCoursesData>> {
    return this.http.get<ApiDataResponseEnvelope<FavoriteCoursesData>>(`${this.api}/Favorite`);
  }

  addFavorite(courseId: number): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.api}/Favorite/${courseId}`, null);
  }

  removeFavorite(courseId: number): Observable<ApiResponseEnvelope> {
    return this.http.delete<ApiResponseEnvelope>(`${this.api}/Favorite/${courseId}`);
  }

  canRate(courseId: number): Observable<ApiDataResponseEnvelope<boolean>> {
    return this.http.get<ApiDataResponseEnvelope<boolean>>(
      `${this.api}/Rating/can-rate/${courseId}`,
    );
  }

  addRating(courseId: number, value: number): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.api}/Rating`, { courseId, value });
  }

  addReview(courseId: number, reviewText: string): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.api}/Review`, { courseId, reviewText });
  }

  deleteReview(reviewId: number): Observable<ApiResponseEnvelope> {
    return this.http.delete<ApiResponseEnvelope>(`${this.api}/Review/${reviewId}`);
  }

  toggleReviewLike(reviewId: number): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.api}/Review/${reviewId}/like`, null);
  }

  getReplies(reviewId: number): Observable<ApiDataResponseEnvelope<CourseReplyDto[]>> {
    return this.http.get<ApiDataResponseEnvelope<CourseReplyDto[]>>(
      `${this.api}/Reply/review/${reviewId}`,
    );
  }

  addReply(reviewId: number, replyText: string): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.api}/Reply/review/${reviewId}`, {
      replyText,
    });
  }

  toggleReplyLike(replyId: number): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.api}/Reply/${replyId}/like`, null);
  }

  deleteReply(replyId: number): Observable<ApiResponseEnvelope> {
    return this.http.delete<ApiResponseEnvelope>(`${this.api}/Reply/${replyId}`);
  }
}
