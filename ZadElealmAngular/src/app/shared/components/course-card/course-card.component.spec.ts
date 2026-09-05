import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { CourseCardComponent } from './course-card.component';

describe('CourseCardComponent', () => {
  it('renders a real link and falls back when the course image fails', () => {
    TestBed.configureTestingModule({
      imports: [CourseCardComponent],
      providers: [provideRouter([])],
    });
    const fixture = TestBed.createComponent(CourseCardComponent);
    fixture.componentRef.setInput('course', {
      id: 9,
      name: 'دورة الفقه',
      description: 'وصف الدورة',
      author: 'الشيخ أحمد',
      courseLanguage: 'العربية',
      courseVideosCount: 8,
      rating: 4.5,
      imageUrl: 'broken.jpg',
      category: { id: 2, name: 'الفقه', description: '', imageUrl: '' },
      createdAt: '2026-01-01',
    });
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('a') as HTMLAnchorElement;
    const image = fixture.nativeElement.querySelector('img') as HTMLImageElement;
    expect(link.getAttribute('href')).toBe('/app/courses/9');
    expect(link.textContent).toContain('الشيخ أحمد');
    expect(link.textContent).toContain('تفاصيل الدورة');

    image.dispatchEvent(new Event('error'));
    fixture.detectChanges();
    expect(image.getAttribute('src')).toBe('assets/brand/course-placeholder.svg');
  });

  it('renders safely and uses the fallback when optional API image and category are missing', () => {
    TestBed.configureTestingModule({
      imports: [CourseCardComponent],
      providers: [provideRouter([])],
    });
    const fixture = TestBed.createComponent(CourseCardComponent);
    fixture.componentRef.setInput('course', {
      id: 10,
      name: 'دورة بلا صورة',
      description: 'وصف الدورة',
      author: 'المحاضر',
      courseLanguage: 'العربية',
      courseVideosCount: 3,
      rating: 0,
      imageUrl: null,
      category: null,
      createdAt: '2026-01-01',
    });

    expect(() => fixture.detectChanges()).not.toThrow();

    const image = fixture.nativeElement.querySelector('img') as HTMLImageElement;
    expect(image.getAttribute('src')).toBe('assets/brand/course-placeholder.svg');
    expect(fixture.nativeElement.textContent).toContain('دورة تعليمية');
  });
});
