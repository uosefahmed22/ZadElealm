import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiDataResponseEnvelope } from '../api/api-error.models';
import { appEnvironment } from '../config/app-environment';
import { CategoryDto, CourseCatalogFilters, PaginatedCoursesResponse } from './catalog.models';

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly categoryUrl = `${appEnvironment.apiBaseUrl}/Category`;

  getCategories(): Observable<ApiDataResponseEnvelope<CategoryDto[]>> {
    return this.http.get<ApiDataResponseEnvelope<CategoryDto[]>>(this.categoryUrl);
  }

  getCourses(filters: CourseCatalogFilters): Observable<PaginatedCoursesResponse> {
    let params = new HttpParams()
      .set('categoryId', filters.categoryId)
      .set('pageNumber', filters.pageNumber)
      .set('pageSize', filters.pageSize)
      .set('sortBy', filters.sortBy)
      .set('sortDirection', filters.sortDirection);

    if (filters.search) {
      params = params.set('search', filters.search);
    }
    if (filters.author) {
      params = params.set('author', filters.author);
    }
    if (filters.language) {
      params = params.set('language', filters.language);
    }
    if (filters.minRating > 0) {
      params = params.set('minRating', filters.minRating);
    }

    return this.http.get<PaginatedCoursesResponse>(`${this.categoryUrl}/get-courses-by-category`, {
      params,
    });
  }
}
