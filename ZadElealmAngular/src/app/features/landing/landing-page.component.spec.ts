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
