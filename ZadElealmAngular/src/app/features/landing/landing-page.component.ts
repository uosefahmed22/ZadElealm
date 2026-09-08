import { ViewportScroller } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  HostListener,
  inject,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter, Subscription } from 'rxjs';

import { CatalogApiService } from '../../core/catalog/catalog-api.service';
import { CategoryDto, CourseDto } from '../../core/catalog/catalog.models';
import { formatArabicNumber } from '../../core/i18n/arabic-number-format.util';
import { CourseCardComponent } from '../../shared/components/course-card/course-card.component';

interface NavItem {
  readonly label: string;
  readonly fragment: string;
}

interface FeatureCard {
  readonly title: string;
  readonly description: string;
  readonly icon: string;
}

interface JourneyStep {
  readonly number: string;
  readonly title: string;
  readonly description: string;
  readonly icon: string;
}

interface LandingCourse {
  readonly source: CourseDto;
  readonly id: number;
  readonly title: string;
  readonly description: string;
  readonly category: string;
  readonly author: string;
  readonly videoCountLabel: string;
  readonly image: string;
  readonly imageAlt: string;
  readonly imageSrcSet: string | null;
  readonly videoUrl: string;
}

interface FaqItem {
  readonly question: string;
  readonly answer: string;
}

type CourseRegionState = 'loading' | 'success' | 'empty' | 'error';

@Component({
  selector: 'app-landing-page',
  imports: [RouterLink, CourseCardComponent],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingPageComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('navbar', { static: true }) private navbarRef?: ElementRef<HTMLElement>;
  @ViewChild('menuToggle', { static: true }) private menuToggleRef?: ElementRef<HTMLButtonElement>;
  @ViewChild('mainNavigation', { static: true })
  private mainNavigationRef?: ElementRef<HTMLElement>;

  private readonly viewportScroller = inject(ViewportScroller);
  private readonly router = inject(Router);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly destroyRef = inject(DestroyRef);
  private resizeObserver?: ResizeObserver;
  private sectionObserver?: IntersectionObserver;
  private readonly routerSubscription: Subscription;
  private observedNavHeight = 0;
  private scrollFrame?: number;
  private readonly fallbackImage = 'assets/brand/course-placeholder.svg';

  private readonly handleWindowScroll = (): void => {
    if (this.scrollFrame !== undefined) return;

    this.scrollFrame = requestAnimationFrame(() => {
      this.scrollFrame = undefined;
      this.updateScrolledState();
      this.updateActiveSectionFromPosition();
    });
  };

  constructor() {
    this.routerSubscription = this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => this.setMobileMenu(false));
  }

  readonly navItems: readonly NavItem[] = [
    { label: 'الرئيسية', fragment: 'home' },
    { label: 'الدورات', fragment: 'courses' },
    { label: 'الأسئلة الشائعة', fragment: 'faq' },
  ];

  readonly featureCards: readonly FeatureCard[] = [
    {
      title: 'دورات منظمة',
      description: 'مناهج مرتبة من الأساس إلى المتقدم في تجربة واضحة ومنظمة.',
      icon: 'book',
    },
    {
      title: 'متابعة التقدم',
      description: 'تتبع مستواك خطوة بخطوة لتصل إلى أهدافك بثبات.',
      icon: 'trend',
    },
    {
      title: 'اختبارات تفاعلية',
      description: 'اختبر فهمك وتأكد من استيعابك للمفاهيم بعد كل درس.',
      icon: 'clipboard',
    },
    {
      title: 'شهادات إتمام',
      description: 'احصل على شهادة إتمام تحفظ إنجازك وتعزز رحلتك العلمية.',
      icon: 'award',
    },
  ];

  readonly journeySteps: readonly JourneyStep[] = [
    {
      number: formatArabicNumber(1),
      title: 'أنشئ حسابك واختر دورتك',
      description: 'سجّل حسابك واختر الدورة المناسبة لمستواك وهدفك.',
      icon: 'user-plus',
    },
    {
      number: formatArabicNumber(2),
      title: 'تعلّم وتدرّب',
      description: 'شاهد الدروس، ونفذ الأنشطة، وحل الاختبارات.',
      icon: 'play-screen',
    },
    {
      number: formatArabicNumber(3),
      title: 'أكمل واحصل على شهادتك',
      description: 'أتم متطلبات الدورة واحصل على شهادتك.',
      icon: 'award',
    },
  ];

  readonly faqItems: readonly FaqItem[] = [
    {
      question: 'كيف أبدأ التعلم في المنصة؟',
      answer:
        'تبدأ بإنشاء حسابك، ثم تختار المسار المناسب لك، وبعدها تتابع الدروس والاختبارات من مكان واحد.',
    },
    {
      question: 'هل الدورات تشمل شهادات إتمام؟',
      answer:
        'المنصة تعرض الشهادات ضمن تجربة التعلم، ويمكنك متابعة إنجازك ومعرفة ما يلزمك لإتمام المسار.',
    },
    {
      question: 'هل يمكنني التعلم من الهاتف؟',
      answer:
        'نعم، الواجهة مصممة لتبقى واضحة على الجوال والتابلت وسطح المكتب مع الحفاظ على ترتيب المحتوى.',
    },
    {
      question: 'كيف أتابع تقدمي في الدورات؟',
      answer:
        'من خلال بطاقات التقدم والاختبارات المرتبطة بالمسار، ترى ما أنجزته وما بقي لك لإكمال الرحلة.',
    },
  ];

  readonly courseSkeletons = [1, 2, 3] as const;
  readonly courses = signal<readonly LandingCourse[]>([]);
  readonly categories = signal<readonly CategoryDto[]>([]);
  readonly featuredCourse = computed(() => this.courses()[0] ?? null);
  readonly courseRegionState = signal<CourseRegionState>('loading');
  readonly openFaqIndex = signal(0);
  readonly activeSection = signal('home');
  readonly mobileMenuOpen = signal(false);
  readonly isScrolled = signal(false);

  ngOnInit(): void {
    this.loadCourses();
    this.loadCategories();
  }

  ngAfterViewInit(): void {
    this.updateNavMetrics();

    const navbar = this.navbarRef?.nativeElement;
    if (navbar && typeof ResizeObserver !== 'undefined') {
      this.resizeObserver = new ResizeObserver(() => this.updateNavMetrics());
      this.resizeObserver.observe(navbar);
    }

    this.updateScrolledState();
    window.addEventListener('scroll', this.handleWindowScroll, { passive: true });
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
    this.sectionObserver?.disconnect();
    this.routerSubscription.unsubscribe();
    window.removeEventListener('scroll', this.handleWindowScroll);
    if (this.scrollFrame !== undefined) cancelAnimationFrame(this.scrollFrame);
    document.body.classList.remove('menu-open');
  }

  loadCourses(): void {
    this.courseRegionState.set('loading');

    this.catalogApi
      .getCourses({
        categoryId: 0,
        search: '',
        author: '',
        language: '',
        minRating: 0,
        sortBy: 'date',
        sortDirection: 'desc',
        pageNumber: 1,
        pageSize: 3,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          const courses = response.data.slice(0, 3).map((course) => this.toLandingCourse(course));
          this.courses.set(courses);
          this.courseRegionState.set(courses.length > 0 ? 'success' : 'empty');
        },
        error: () => {
          this.courses.set([]);
          this.courseRegionState.set('error');
        },
      });
  }

  private loadCategories(): void {
    this.catalogApi
      .getCategories()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => this.categories.set(response.data.slice(0, 5)),
        error: () => this.categories.set([]),
      });
  }

  toggleMobileMenu(): void {
    this.setMobileMenu(!this.mobileMenuOpen(), this.mobileMenuOpen());
  }

  @HostListener('document:keydown', ['$event'])
  handleDocumentKeydown(event: KeyboardEvent): void {
    if (!this.mobileMenuOpen()) return;

    if (event.key === 'Escape') {
      event.preventDefault();
      this.setMobileMenu(false, true);
      return;
    }

    if (event.key !== 'Tab') return;

    const focusableElements = this.getMenuFocusableElements();
    if (focusableElements.length === 0) return;

    const first = focusableElements[0];
    const last = focusableElements[focusableElements.length - 1];
    const active = document.activeElement;

    if (event.shiftKey && active === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && active === last) {
      event.preventDefault();
      first.focus();
    }
  }

  @HostListener('window:resize')
  closeMobileMenuAtDesktopWidth(): void {
    if (window.innerWidth >= 1024) this.setMobileMenu(false);
  }

  navigateToSection(event: Event, fragment: string): void {
    event.preventDefault();
    this.setMobileMenu(false);

    requestAnimationFrame(() => {
      this.updateNavMetrics();
      const target = document.getElementById(fragment);
      const navbar = this.navbarRef?.nativeElement;
      if (!target || !navbar) return;

      const top = target.getBoundingClientRect().top + window.scrollY - navbar.offsetHeight - 24;
      const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

      window.scrollTo({ top: Math.max(0, top), behavior: reducedMotion ? 'auto' : 'smooth' });
      history.replaceState(null, '', `#${fragment}`);
      this.activeSection.set(fragment);
    });
  }

  toggleFaq(index: number): void {
    this.openFaqIndex.update((current) => (current === index ? -1 : index));
  }

  useCourseImageFallback(event: Event): void {
    if (!(event.target instanceof HTMLImageElement)) return;
    if (event.target.src.endsWith(this.fallbackImage)) return;

    event.target.removeAttribute('srcset');
    event.target.src = this.fallbackImage;
  }

  private toLandingCourse(course: CourseDto): LandingCourse {
    const image = course.imageUrl?.trim() || this.fallbackImage;
    return {
      source: course,
      id: course.id,
      title: course.name,
      description: course.description,
      category: course.category?.name || 'دورة تعليمية',
      author: course.author,
      videoCountLabel: `${formatArabicNumber(course.courseVideosCount)} درسًا`,
      image,
      imageAlt: `صورة دورة ${course.name}`,
      imageSrcSet: this.createYouTubeSrcSet(image),
      videoUrl: this.createYouTubeWatchUrl(image) ?? '/register',
    };
  }

  private createYouTubeWatchUrl(imageUrl: string): string | null {
    const match = imageUrl.match(/i\.ytimg\.com\/vi\/([^/]+)\//i);
    return match?.[1] ? `https://www.youtube.com/watch?v=${match[1]}` : null;
  }

  private createYouTubeSrcSet(imageUrl: string): string | null {
    const match = imageUrl.match(/i\.ytimg\.com\/vi\/([^/]+)\//i);
    const videoId = match?.[1];
    if (!videoId) return null;

    return `https://i.ytimg.com/vi/${videoId}/mqdefault.jpg 320w, https://i.ytimg.com/vi/${videoId}/hqdefault.jpg 480w`;
  }

  private updateNavMetrics(): void {
    const navbar = this.navbarRef?.nativeElement;
    if (!navbar) return;

    const navHeight = navbar.offsetHeight;
    document.documentElement.style.setProperty('--nav-h', `${navHeight}px`);
    this.viewportScroller.setOffset([0, navHeight + 24]);

    if (navHeight !== this.observedNavHeight) {
      this.observedNavHeight = navHeight;
      this.setupSectionObserver(navHeight);
    }
  }

  private setupSectionObserver(navHeight: number): void {
    this.sectionObserver?.disconnect();
    if (typeof IntersectionObserver === 'undefined') return;

    this.sectionObserver = new IntersectionObserver(() => this.updateActiveSectionFromPosition(), {
      rootMargin: `-${navHeight + 40}px 0px -60% 0px`,
      threshold: [0, 0.1, 0.25, 0.5],
    });

    for (const item of this.navItems) {
      const section = document.getElementById(item.fragment);
      if (section) this.sectionObserver.observe(section);
    }
  }

  private updateScrolledState(): void {
    this.isScrolled.set(window.scrollY > 40);
  }

  private setMobileMenu(isOpen: boolean, restoreFocus = false): void {
    const wasOpen = this.mobileMenuOpen();
    this.mobileMenuOpen.set(isOpen);
    document.body.classList.toggle('menu-open', isOpen);

    if (isOpen) {
      requestAnimationFrame(() => this.getMenuFocusableElements()[0]?.focus());
    } else if (restoreFocus && wasOpen) {
      requestAnimationFrame(() => this.menuToggleRef?.nativeElement.focus());
    }
  }

  private getMenuFocusableElements(): HTMLElement[] {
    const navigation = this.mainNavigationRef?.nativeElement;
    if (!navigation) return [];

    return Array.from(
      navigation.querySelectorAll<HTMLElement>('a[href], button:not([disabled]), [tabindex="0"]'),
    ).filter((element) => element.getAttribute('aria-hidden') !== 'true');
  }

  private updateActiveSectionFromPosition(): void {
    const navbar = this.navbarRef?.nativeElement;
    if (!navbar) return;

    const marker = navbar.offsetHeight + 40;
    let currentSection = this.navItems[0].fragment;

    for (const item of this.navItems) {
      const section = document.getElementById(item.fragment);
      if (section && section.getBoundingClientRect().top <= marker) {
        currentSection = item.fragment;
      }
    }

    this.activeSection.set(currentSection);
  }
}
