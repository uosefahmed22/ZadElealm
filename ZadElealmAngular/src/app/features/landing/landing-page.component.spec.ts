import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { routes } from '../../app.routes';
import { LandingPageComponent } from './landing-page.component';

describe('LandingPageComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LandingPageComponent],
      providers: [provideRouter(routes)],
    }).compileComponents();
  });

  it('renders the landing page headline', () => {
    const fixture = TestBed.createComponent(LandingPageComponent);
    fixture.detectChanges();

    const headline = fixture.nativeElement.querySelector('h1') as HTMLHeadingElement | null;
    expect(headline?.textContent).toContain('تعلّم العلوم الإسلامية');
  });

  it('renders visible headings for every navigation target', () => {
    const fixture = TestBed.createComponent(LandingPageComponent);
    fixture.detectChanges();

    const targets = ['home', 'features', 'how-it-works', 'faq'];
    const headings = targets.map((target) =>
      fixture.nativeElement.querySelector(`section#${target} h1, section#${target} h2`),
    );

    expect(headings.every(Boolean)).toBe(true);
    expect(fixture.nativeElement.querySelector('#features-title')?.textContent).toContain(
      'المميزات',
    );
    expect(fixture.nativeElement.textContent).toContain('كل ما تحتاجه لرحلة تعليمية منظمة وواضحة');
  });

  it('marks the home navigation link as current initially', () => {
    const fixture = TestBed.createComponent(LandingPageComponent);
    fixture.detectChanges();

    const currentLink = fixture.nativeElement.querySelector(
      '.nav-link[aria-current="page"]',
    ) as HTMLAnchorElement | null;

    expect(currentLink?.getAttribute('href')).toBe('#home');
    expect(currentLink?.textContent).toContain('الرئيسية');
  });

  it('opens and closes the mobile navigation menu', () => {
    const fixture = TestBed.createComponent(LandingPageComponent);
    fixture.detectChanges();

    const menuButton = fixture.nativeElement.querySelector(
      '.mobile-menu-toggle',
    ) as HTMLButtonElement;

    menuButton.click();
    fixture.detectChanges();
    expect(menuButton.getAttribute('aria-expanded')).toBe('true');
    expect(fixture.nativeElement.querySelector('.site-nav')?.classList).toContain('is-open');

    menuButton.click();
    fixture.detectChanges();
    expect(menuButton.getAttribute('aria-expanded')).toBe('false');
  });

  it('renders the footer after the FAQ section', () => {
    const fixture = TestBed.createComponent(LandingPageComponent);
    fixture.detectChanges();

    const footer = fixture.nativeElement.querySelector(
      'main + footer.site-footer',
    ) as HTMLElement | null;

    expect(footer).toBeTruthy();
    expect(footer?.textContent).toContain('ابدأ رحلتك في طلب العلم اليوم');
  });

  it('maps the root route directly to the landing page', () => {
    const rootRoute = routes.find((route) => route.path === '' && route.pathMatch === 'full');

    expect(rootRoute?.component).toBe(LandingPageComponent);
  });

  it('points the primary calls to action to auth routes', () => {
    const fixture = TestBed.createComponent(LandingPageComponent);
    fixture.detectChanges();

    const loginLink = fixture.nativeElement.querySelector(
      'a[routerlink="/login"]',
    ) as HTMLAnchorElement | null;
    const registerLinks = Array.from(
      fixture.nativeElement.querySelectorAll('a[routerlink="/register"]'),
    ) as HTMLAnchorElement[];

    expect(loginLink?.getAttribute('href')).toBe('/login');
    expect(registerLinks.some((link) => link.getAttribute('href') === '/register')).toBe(true);
  });
});
