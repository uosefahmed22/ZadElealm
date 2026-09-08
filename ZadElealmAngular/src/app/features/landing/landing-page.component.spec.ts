import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Subject, of, throwError } from 'rxjs';

import { routes } from '../../app.routes';
import { CatalogApiService } from '../../core/catalog/catalog-api.service';
import { CategoryDto, PaginatedCoursesResponse } from '../../core/catalog/catalog.models';
import { LandingPageComponent } from './landing-page.component';

describe('LandingPageComponent', () => {
  let catalogApi: {
    getCourses: ReturnType<typeof vi.fn>;
    getCategories: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    catalogApi = {
      getCourses: vi.fn(() => of(coursesResponse())),
      getCategories: vi.fn(() => of({ statusCode: 200, data: categories() })),
    };

    await TestBed.configureTestingModule({
      imports: [LandingPageComponent],
      providers: [provideRouter(routes), { provide: CatalogApiService, useValue: catalogApi }],
    }).compileComponents();
  });

  it('renders the source-of-truth headline and one header primary CTA', () => {
    const fixture = createFixture();
    const headline = fixture.nativeElement.querySelector('h1') as HTMLHeadingElement;
    const headerPrimaryCtas = fixture.nativeElement.querySelectorAll(
      '.site-header .zad-btn--primary',
    );

    expect(headline.textContent?.trim()).toBe('تعلّم العلوم الإسلامية بخطوات واضحة');
    expect(headerPrimaryCtas).toHaveLength(1);
    expect(headerPrimaryCtas[0].textContent.trim()).toBe('ابدأ رحلتك');
  });

  it('prioritizes course discovery in the primary navigation', () => {
    const fixture = createFixture();
    const navigationLinks = Array.from(
      fixture.nativeElement.querySelectorAll('.site-nav .nav-link'),
    ) as HTMLAnchorElement[];
    const courseLink = navigationLinks.find((link) => link.textContent?.trim() === 'الدورات');

    expect(courseLink?.getAttribute('href')).toBe('#courses');
    expect(navigationLinks.some((link) => link.textContent?.trim() === 'المميزات')).toBe(false);
  });

  it('renders API course data without invented duration or progress', () => {
    const fixture = createFixture();
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('أساسيات التجويد');
    expect(text).toContain('١٢ درسًا');
    expect(text).toContain('أحمد محمود');
    expect(text).toContain('٤٫٨');
    expect(fixture.nativeElement.querySelector('.course-progress')).toBeNull();
    expect(fixture.nativeElement.querySelector('[role="progressbar"]')).toBeNull();
  });

  it('turns a real API course video thumbnail into the hero video spotlight', () => {
    const fixture = createFixture();
    const spotlight = fixture.nativeElement.querySelector('.video-window') as HTMLAnchorElement;
    const image = spotlight.querySelector('img') as HTMLImageElement;

    expect(spotlight.getAttribute('href')).toBe('https://www.youtube.com/watch?v=test-video');
    expect(spotlight.getAttribute('target')).toBe('_blank');
    expect(image.getAttribute('src')).toBe('https://i.ytimg.com/vi/test-video/hqdefault.jpg');
    expect(spotlight.textContent).toContain('أساسيات التجويد');
  });

  it('keeps the landing journey concise and renders API footer categories', () => {
    const fixture = createFixture();
    const text = fixture.nativeElement.textContent as string;

    expect(fixture.nativeElement.querySelectorAll('.journey-step')).toHaveLength(3);
    expect(fixture.nativeElement.querySelector('.audience-section')).toBeNull();
    expect(fixture.nativeElement.querySelector('.challenge-section')).toBeNull();
    expect(text).toContain('أنشئ حسابك واختر دورتك');
    expect(text).toContain('القرآن الكريم');
    expect(catalogApi.getCategories).toHaveBeenCalledOnce();
  });

  it('uses the certificate preview rendered from the backend PDF document', () => {
    const fixture = createFixture();
    const image = fixture.nativeElement.querySelector('.certificate-document img') as HTMLImageElement;

    expect(image.getAttribute('src')).toBe('assets/brand/certificate-preview.png');
    expect(image.getAttribute('alt')).toContain('صادرة من نظام زاد تعلم');
    expect(image.getAttribute('loading')).toBe('lazy');
    expect(fixture.nativeElement.textContent).not.toContain('جميع الحقوق محفوظة');
  });

  it('uses responsive lazy course images and a canonical YouTube srcset', () => {
    const fixture = createFixture();
    const image = fixture.nativeElement.querySelector('.course-media img') as HTMLImageElement;

    expect(image.getAttribute('loading')).toBe('lazy');
    expect(image.getAttribute('decoding')).toBe('async');
    expect(image.getAttribute('width')).toBe('480');
    expect(image.getAttribute('height')).toBe('270');
    expect(image.getAttribute('srcset')).toContain('mqdefault.jpg 320w');
    expect(image.getAttribute('srcset')).toContain('hqdefault.jpg 480w');
  });

  it('replaces a failed external course image with the local fallback', () => {
    const fixture = createFixture();
    const image = fixture.nativeElement.querySelector('.course-media img') as HTMLImageElement;

    image.dispatchEvent(new Event('error'));
    fixture.detectChanges();

    expect(image.getAttribute('src')).toBe('assets/brand/course-placeholder.svg');
    expect(image.hasAttribute('srcset')).toBe(false);
  });

  it('shows box-matched skeleton cards while the catalog request is pending', () => {
    const pending = new Subject<PaginatedCoursesResponse>();
    catalogApi.getCourses.mockReturnValue(pending.asObservable());
    const fixture = createFixture();

    expect(fixture.nativeElement.querySelectorAll('.course-card--skeleton')).toHaveLength(3);
    expect(fixture.nativeElement.querySelector('.course-region').getAttribute('aria-busy')).toBe(
      'true',
    );
  });

  it('shows a clear empty state', () => {
    catalogApi.getCourses.mockReturnValue(of(coursesResponse([])));
    const fixture = createFixture();

    expect(fixture.nativeElement.textContent).toContain('لا توجد دورات متاحة الآن');
    expect(fixture.nativeElement.querySelectorAll('.course-card')).toHaveLength(0);
  });

  it('shows an Arabic error state and retries the real catalog request', () => {
    catalogApi.getCourses.mockReturnValue(throwError(() => new Error('offline')));
    const fixture = createFixture();

    expect(fixture.nativeElement.textContent).toContain('تعذر تحميل الدورات');
    const retry = fixture.nativeElement.querySelector(
      '.course-message button',
    ) as HTMLButtonElement;
    catalogApi.getCourses.mockReturnValue(of(coursesResponse()));
    retry.click();
    fixture.detectChanges();

    expect(catalogApi.getCourses).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('أساسيات التجويد');
  });

  it('opens the mobile navigation, closes it with Escape, and restores the trigger', async () => {
    const fixture = createFixture();
    const menuButton = fixture.nativeElement.querySelector(
      '.mobile-menu-toggle',
    ) as HTMLButtonElement;

    menuButton.click();
    fixture.detectChanges();
    expect(menuButton.getAttribute('aria-expanded')).toBe('true');
    expect(document.body.classList).toContain('menu-open');

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();
    await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()));

    expect(menuButton.getAttribute('aria-expanded')).toBe('false');
    expect(document.body.classList).not.toContain('menu-open');
    expect(document.activeElement).toBe(menuButton);
  });

  it('keeps FAQ controls connected to labelled regions', () => {
    const fixture = createFixture();
    const trigger = fixture.nativeElement.querySelector('.faq-trigger') as HTMLButtonElement;
    const panel = fixture.nativeElement.querySelector(
      `#${trigger.getAttribute('aria-controls')}`,
    ) as HTMLElement;

    expect(trigger.getAttribute('aria-expanded')).toBe('true');
    expect(panel.getAttribute('role')).toBe('region');
    expect(panel.getAttribute('aria-labelledby')).toBe(trigger.id);
  });

  it('maps the root route directly to the landing page', () => {
    const rootRoute = routes.find((route) => route.path === '' && route.pathMatch === 'full');
    expect(rootRoute?.component).toBe(LandingPageComponent);
  });

  it('links course discovery to the existing protected catalog route', () => {
    const fixture = createFixture();
    const courseLinks = Array.from(
      fixture.nativeElement.querySelectorAll('a[routerlink="/app/courses"]'),
    ) as HTMLAnchorElement[];

    expect(courseLinks.length).toBeGreaterThanOrEqual(2);
    expect(courseLinks.every((link) => link.getAttribute('href') === '/app/courses')).toBe(true);
  });
});

function createFixture() {
  const fixture = TestBed.createComponent(LandingPageComponent);
  fixture.detectChanges();
  return fixture;
}

function coursesResponse(
  data: PaginatedCoursesResponse['data'] = [course()],
): PaginatedCoursesResponse {
  return {
    statusCode: 200,
    data,
    metaData: {
      pageSize: 3,
      currentPage: 1,
      totalMatchedItems: data.length,
      nextPage: null,
      previousPage: null,
      numberOfPages: data.length > 0 ? 1 : 0,
    },
  };
}

function course(): PaginatedCoursesResponse['data'][number] {
  return {
    id: 10,
    name: 'أساسيات التجويد',
    description: 'مدخل عملي إلى أحكام التجويد',
    author: 'أحمد محمود',
    courseLanguage: 'العربية',
    courseVideosCount: 12,
    rating: 4.8,
    imageUrl: 'https://i.ytimg.com/vi/test-video/hqdefault.jpg',
    category: { id: 3, name: 'القرآن الكريم', description: 'دورات القرآن', imageUrl: '' },
    createdAt: '2026-01-10T00:00:00',
  };
}

function categories(): CategoryDto[] {
  return [{ id: 3, name: 'القرآن الكريم', description: 'دورات القرآن', imageUrl: '' }];
}
