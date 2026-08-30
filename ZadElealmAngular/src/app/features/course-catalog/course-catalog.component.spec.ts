import { TestBed } from '@angular/core/testing';
import { Observable, Subject, of, throwError } from 'rxjs';

import { ApiDataResponseEnvelope } from '../../core/api/api-error.models';
import { CatalogApiService } from '../../core/catalog/catalog-api.service';
import {
  CategoryDto,
  CourseCatalogFilters,
  PaginatedCoursesResponse,
} from '../../core/catalog/catalog.models';
import { CourseCatalogComponent } from './course-catalog.component';

describe('CourseCatalogComponent', () => {
  let catalogApi: {
    getCategories: ReturnType<typeof vi.fn>;
    getCourses: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    catalogApi = {
      getCategories: vi.fn(() => of(categoriesResponse())),
      getCourses: vi.fn(() => of(coursesResponse())),
    };

    await TestBed.configureTestingModule({
      imports: [CourseCatalogComponent],
      providers: [{ provide: CatalogApiService, useValue: catalogApi }],
    }).compileComponents();
  });

  it('renders real course data and the result count', () => {
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('أساسيات التجويد');
    expect(fixture.nativeElement.textContent).toContain('أحمد محمود');
    expect(fixture.nativeElement.textContent).toContain('دورة واحدة');
  });

  it('maps submitted filters to the backend contract', () => {
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.filterForm.patchValue({
      search: ' تجويد ',
      categoryId: 3,
      author: ' أحمد ',
      language: 'العربية',
      minRating: 4,
      sort: 'rating-desc',
    });
    component.applyFilters();

    const filters = catalogApi.getCourses.mock.calls.at(-1)?.[0] as CourseCatalogFilters;
    expect(filters).toMatchObject({
      search: 'تجويد',
      categoryId: 3,
      author: 'أحمد',
      language: 'العربية',
      minRating: 4,
      sortBy: 'rating',
      sortDirection: 'desc',
      pageNumber: 1,
      pageSize: 9,
    });
  });

  it('requests the selected page without losing filters', () => {
    catalogApi.getCourses.mockReturnValue(of(coursesResponse({ numberOfPages: 3, nextPage: 2 })));
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();

    fixture.componentInstance.changePage(2);

    const filters = catalogApi.getCourses.mock.calls.at(-1)?.[0] as CourseCatalogFilters;
    expect(filters.pageNumber).toBe(2);
  });

  it('shows skeleton cards while the catalog request is pending', () => {
    const pending = new Subject<PaginatedCoursesResponse>();
    catalogApi.getCourses.mockReturnValue(pending.asObservable());
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.skeleton-card')).toHaveLength(6);
  });

  it('shows an actionable empty state', () => {
    catalogApi.getCourses.mockReturnValue(of(coursesResponse({}, [])));
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('لا توجد دورات مطابقة');
    expect(fixture.nativeElement.textContent).toContain('عرض كل الدورات');
  });

  it('shows the normalized api error and lets the user retry', () => {
    catalogApi.getCourses.mockReturnValue(throwError(() => new Error('catalog unavailable')));
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('catalog unavailable');
    expect(fixture.nativeElement.textContent).toContain('حاول مرة أخرى');
  });
});

function categoriesResponse(): ApiDataResponseEnvelope<CategoryDto[]> {
  return {
    statusCode: 200,
    data: [
      {
        id: 3,
        name: 'القرآن الكريم',
        description: 'دورات القرآن الكريم',
        imageUrl: 'category.jpg',
      },
    ],
  };
}

function coursesResponse(
  metadata: Partial<PaginatedCoursesResponse['metaData']> = {},
  data: PaginatedCoursesResponse['data'] = [
    {
      id: 10,
      name: 'أساسيات التجويد',
      description: 'مدخل عملي إلى أحكام التجويد',
      author: 'أحمد محمود',
      courseLanguage: 'العربية',
      courseVideosCount: 12,
      rating: 4.8,
      imageUrl: 'course.jpg',
      category: categoriesResponse().data[0],
      createdAt: '2026-01-10T00:00:00',
    },
  ],
): PaginatedCoursesResponse {
  return {
    statusCode: 200,
    data,
    metaData: {
      pageSize: 9,
      currentPage: 1,
      totalMatchedItems: data.length,
      nextPage: null,
      previousPage: null,
      numberOfPages: 1,
      ...metadata,
    },
  };
}
