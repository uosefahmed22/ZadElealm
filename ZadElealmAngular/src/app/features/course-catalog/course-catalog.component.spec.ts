import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, Subject, of, throwError } from 'rxjs';

import { ApiDataResponseEnvelope } from '../../core/api/api-error.models';
import { CatalogApiService } from '../../core/catalog/catalog-api.service';
import {
  CategoryDto,
  CourseCatalogFilters,
  PaginatedCoursesResponse,
} from '../../core/catalog/catalog.models';
import { CourseCatalogComponent } from './course-catalog.component';
import { LearningApiService } from '../../core/learning/learning-api.service';

describe('CourseCatalogComponent', () => {
  let catalogApi: {
    getCategories: ReturnType<typeof vi.fn>;
    getCourses: ReturnType<typeof vi.fn>;
  };
  let learningApi: {
    getFavorites: ReturnType<typeof vi.fn>;
    getEnrolledCourses: ReturnType<typeof vi.fn>;
    enroll: ReturnType<typeof vi.fn>;
    addFavorite: ReturnType<typeof vi.fn>;
    removeFavorite: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    catalogApi = {
      getCategories: vi.fn(() => of(categoriesResponse())),
      getCourses: vi.fn(() => of(coursesResponse())),
    };
    learningApi = {
      getFavorites: vi.fn(() =>
        of({ statusCode: 200, data: { courses: [], allFavoriteCourses: 0 } }),
      ),
      getEnrolledCourses: vi.fn(() =>
        of({ statusCode: 200, data: { courses: [], progress: [], allEnrolledCourses: 0 } }),
      ),
      enroll: vi.fn(() => of({ statusCode: 200 })),
      addFavorite: vi.fn(() => of({ statusCode: 200 })),
      removeFavorite: vi.fn(() => of({ statusCode: 200 })),
    };

    await TestBed.configureTestingModule({
      imports: [CourseCatalogComponent],
      providers: [
        provideRouter([]),
        { provide: CatalogApiService, useValue: catalogApi },
        { provide: LearningApiService, useValue: learningApi },
      ],
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

    expect(fixture.nativeElement.textContent).toContain('لا توجد دورات في هذا التصنيف حاليًا');
    expect(fixture.nativeElement.textContent).toContain('عرض كل الدورات');
  });

  it('shows the normalized api error and lets the user retry', () => {
    catalogApi.getCourses.mockReturnValue(throwError(() => new Error('catalog unavailable')));
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('catalog unavailable');
    expect(fixture.nativeElement.textContent).toContain('إعادة المحاولة');
  });

  it('debounces search for 300ms and keeps the route category', () => {
    vi.useFakeTimers();
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    component.filterForm.controls.categoryId.setValue(3);
    component.applyFilters();
    const callsAfterCategory = catalogApi.getCourses.mock.calls.length;

    component.filterForm.controls.search.setValue('تجويد');
    vi.advanceTimersByTime(299);
    expect(catalogApi.getCourses).toHaveBeenCalledTimes(callsAfterCategory);
    vi.advanceTimersByTime(1);

    const filters = catalogApi.getCourses.mock.calls.at(-1)?.[0] as CourseCatalogFilters;
    expect(filters).toMatchObject({ categoryId: 3, search: 'تجويد' });
    vi.useRealTimers();
  });

  it('adds and removes a course from favorites using the real course id', () => {
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const selectedCourse = coursesResponse().data[0];

    component.toggleFavorite(selectedCourse);
    expect(learningApi.addFavorite).toHaveBeenCalledWith(selectedCourse.id);
    expect(component.favoriteIds().has(selectedCourse.id)).toBe(true);

    component.toggleFavorite(selectedCourse);
    expect(learningApi.removeFavorite).toHaveBeenCalledWith(selectedCourse.id);
    expect(component.favoriteIds().has(selectedCourse.id)).toBe(false);
  });

  it('uses enrollment as the primary card action and disables it after success', () => {
    const fixture = TestBed.createComponent(CourseCatalogComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const selectedCourse = coursesResponse().data[0];
    const enrollButton = fixture.nativeElement.querySelector('.enroll-button') as HTMLButtonElement;
    const favoriteButton = fixture.nativeElement.querySelector(
      '.favorite-heart',
    ) as HTMLButtonElement;

    expect(enrollButton.textContent).toContain('التسجيل في الدورة');
    expect(favoriteButton.getAttribute('aria-label')).toContain('إضافة أساسيات التجويد');

    enrollButton.click();
    fixture.detectChanges();

    expect(learningApi.enroll).toHaveBeenCalledWith(selectedCourse.id);
    expect(component.enrolledCourseIds().has(selectedCourse.id)).toBe(true);
    expect(enrollButton.textContent).toContain('مسجل بالفعل');
    expect(enrollButton.disabled).toBe(true);
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
