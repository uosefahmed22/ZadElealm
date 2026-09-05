import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { CatalogApiService } from '../../core/catalog/catalog-api.service';
import { CourseCategoriesComponent } from './course-categories.component';

describe('CourseCategoriesComponent', () => {
  it('renders real categories and links each one to its filtered catalog', () => {
    TestBed.configureTestingModule({
      imports: [CourseCategoriesComponent],
      providers: [
        provideRouter([]),
        {
          provide: CatalogApiService,
          useValue: {
            getCategories: () =>
              of({
                statusCode: 200,
                data: [
                  {
                    id: 3,
                    name: 'القرآن الكريم',
                    description: 'دورات القرآن الكريم',
                    imageUrl: 'http://res.cloudinary.com/example/category.jpg',
                  },
                ],
              }),
          },
        },
      ],
    });

    const fixture = TestBed.createComponent(CourseCategoriesComponent);
    fixture.detectChanges();
    const categoryLink = fixture.nativeElement.querySelector('.category-card') as HTMLAnchorElement;
    const categoryImage = fixture.nativeElement.querySelector(
      '.category-card img',
    ) as HTMLImageElement;

    expect(fixture.nativeElement.textContent).toContain('القرآن الكريم');
    expect(categoryLink.getAttribute('href')).toBe('/app/courses/catalog?categoryId=3');
    expect(categoryImage.getAttribute('src')).toBe(
      'https://res.cloudinary.com/example/category.jpg',
    );
  });
});
