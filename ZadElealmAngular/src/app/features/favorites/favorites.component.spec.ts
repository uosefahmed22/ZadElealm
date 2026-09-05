import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { LearningApiService } from '../../core/learning/learning-api.service';
import { FavoritesComponent } from './favorites.component';

describe('FavoritesComponent', () => {
  let learningApi: {
    getFavorites: ReturnType<typeof vi.fn>;
    removeFavorite: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    learningApi = {
      getFavorites: vi.fn(() =>
        of({ statusCode: 200, data: { courses: [course()], allFavoriteCourses: 1 } }),
      ),
      removeFavorite: vi.fn(() => of({ statusCode: 200 })),
    };

    await TestBed.configureTestingModule({
      imports: [FavoritesComponent],
      providers: [provideRouter([]), { provide: LearningApiService, useValue: learningApi }],
    }).compileComponents();
  });

  it('renders favorites from the API contract', () => {
    const fixture = TestBed.createComponent(FavoritesComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('أساسيات التجويد');
    expect(fixture.nativeElement.querySelector('img').getAttribute('loading')).toBe('eager');
  });

  it('removes a favorite and updates the empty state', () => {
    const fixture = TestBed.createComponent(FavoritesComponent);
    fixture.detectChanges();

    fixture.componentInstance.remove(course());
    fixture.detectChanges();

    expect(learningApi.removeFavorite).toHaveBeenCalledWith(10);
    expect(fixture.nativeElement.textContent).toContain('لا توجد دورات مفضلة بعد');
  });
});

function course() {
  return {
    id: 10,
    name: 'أساسيات التجويد',
    description: 'وصف',
    author: 'أحمد محمود',
    courseLanguage: 'العربية',
    courseVideosCount: 2,
    rating: 5,
    imageUrl: 'course.jpg',
    category: { id: 1, name: 'القرآن', description: '', imageUrl: '' },
    createdAt: '2026-01-01',
  };
}
