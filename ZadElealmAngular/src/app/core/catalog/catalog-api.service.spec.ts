import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { appEnvironment } from '../config/app-environment';
import { CatalogApiService } from './catalog-api.service';

describe('CatalogApiService', () => {
  let service: CatalogApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CatalogApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('gets categories from the real category route', () => {
    service.getCategories().subscribe();

    const request = httpMock.expectOne(`${appEnvironment.apiBaseUrl}/Category`);
    expect(request.request.method).toBe('GET');
    request.flush({ statusCode: 200, data: [] });
  });

  it('sends the supported catalog filters and omits empty optional values', () => {
    service
      .getCourses({
        categoryId: 4,
        search: 'تجويد',
        author: '',
        language: 'العربية',
        minRating: 4,
        sortBy: 'rating',
        sortDirection: 'desc',
        pageNumber: 2,
        pageSize: 9,
      })
      .subscribe();

    const request = httpMock.expectOne(
      (candidate) =>
        candidate.url === `${appEnvironment.apiBaseUrl}/Category/get-courses-by-category`,
    );
    expect(request.request.params.get('categoryId')).toBe('4');
    expect(request.request.params.get('search')).toBe('تجويد');
    expect(request.request.params.has('author')).toBe(false);
    expect(request.request.params.get('language')).toBe('العربية');
    expect(request.request.params.get('minRating')).toBe('4');
    expect(request.request.params.get('sortBy')).toBe('rating');
    expect(request.request.params.get('sortDirection')).toBe('desc');
    expect(request.request.params.get('pageNumber')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('9');
    request.flush({
      statusCode: 200,
      data: [],
      metaData: {
        pageSize: 9,
        currentPage: 2,
        totalMatchedItems: 0,
        nextPage: null,
        previousPage: 1,
        numberOfPages: 1,
      },
    });
  });
});
