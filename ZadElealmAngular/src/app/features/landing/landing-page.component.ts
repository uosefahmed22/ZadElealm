import { CommonModule, ViewportScroller } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  HostListener,
  inject,
  OnDestroy,
  signal,
  ViewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';

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

interface CoursePath {
  readonly level: string;
  readonly title: string;
  readonly description: string;
  readonly image: string;
  readonly imageAlt: string;
}

interface FaqItem {
  readonly question: string;
  readonly answer: string;
}

@Component({
  selector: 'app-landing-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss',
})
export class LandingPageComponent implements AfterViewInit, OnDestroy {
  @ViewChild('navbar', { static: true }) private navbarRef!: ElementRef<HTMLElement>;

  private readonly viewportScroller = inject(ViewportScroller);
  private resizeObserver?: ResizeObserver;
  private sectionObserver?: IntersectionObserver;
  private observedNavHeight = 0;

  readonly navItems: readonly NavItem[] = [
    { label: 'الرئيسية', fragment: 'home' },
    { label: 'المميزات', fragment: 'features' },
    { label: 'كيف تبدأ', fragment: 'how-it-works' },
    { label: 'الأسئلة الشائعة', fragment: 'faq' },
  ];

  readonly featureCards: readonly FeatureCard[] = [
    {
      title: 'دورات منظمة',
      description: 'مناهج مرتبة من الأساس إلى المتقدم في تجربة واضحة ومنظمة.',
      icon: 'assets/icons/book-open.svg',
    },
    {
      title: 'متابعة التقدم',
      description: 'تتبع مستواك خطوة بخطوة لتصل إلى أهدافك بثبات.',
      icon: 'assets/icons/trending-up.svg',
    },
    {
      title: 'اختبارات تفاعلية',
      description: 'اختبر فهمك وتأكد من استيعابك للمفاهيم بعد كل درس.',
      icon: 'assets/icons/clipboard-check.svg',
    },
    {
      title: 'شهادات إتمام',
      description: 'احصل على شهادة إتمام تحفظ إنجازك وتعزز رحلتك العلمية.',
      icon: 'assets/icons/award.svg',
    },
  ];

  readonly journeySteps: readonly JourneyStep[] = [
    {
      number: '1',
      title: 'سجل حسابك',
      description: 'أنشئ حسابك الآن وانضم إلى مجتمع المتعلمين.',
      icon: 'assets/icons/user-plus.svg',
    },
    {
      number: '2',
      title: 'اختر مساراتك',
      description: 'اختر الدورة المناسبة لمستواك وهدفك.',
      icon: 'assets/icons/book-open.svg',
    },
    {
      number: '3',
      title: 'تعلّم وتدرّب',
      description: 'شاهد الدروس، ونفذ الأنشطة، وحل الاختبارات.',
      icon: 'assets/icons/monitor-play.svg',
    },
    {
      number: '4',
      title: 'أكمل واحصل على شهادتك',
      description: 'أتم متطلبات الدورة واحصل على شهادتك.',
      icon: 'assets/icons/award.svg',
    },
  ];

  readonly coursePaths: readonly CoursePath[] = [
    {
      level: 'مبتدئ',
      title: 'أحكام التلاوة',
      description: 'مثال لمسار منظم يبدأ من القواعد الأساسية وينتقل بك إلى قراءة أكثر صحة.',
      image: 'assets/illustrations/course-quran.svg',
      imageAlt: 'مصحف مفتوح على حامل خشبي',
    },
    {
      level: 'مبتدئ',
      title: 'اللغة العربية',
      description: 'تعلم أساسيات النحو والصرف بطريقة واضحة ومبسطة.',
      image: 'assets/illustrations/course-arabic.svg',
      imageAlt: 'مخطوطة عربية وقلم',
    },
    {
      level: 'متوسط',
      title: 'السيرة النبوية',
      description: 'مثال لمسار يربط بين التعلم والمعنى التربوي في رحلة متسلسلة ومريحة.',
      image: 'assets/illustrations/course-seerah.svg',
      imageAlt: 'القبة الخضراء في المدينة المنورة',
    },
  ];

  readonly progressChecklist = [
    { label: 'استكمال الدروس المنظمة', completed: true },
    { label: 'حل الاختبارات التفاعلية', completed: true },
    { label: 'متابعة التقدم داخل المسار', completed: true },
    { label: 'الحصول على الشهادة بعد الإنجاز', completed: false },
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

  readonly openFaqIndex = signal(0);
  readonly activeSection = signal('home');
  readonly mobileMenuOpen = signal(false);
  readonly isScrolled = signal(false);

  ngAfterViewInit(): void {
    this.updateNavMetrics();

    if (typeof ResizeObserver !== 'undefined') {
      this.resizeObserver = new ResizeObserver(() => this.updateNavMetrics());
      this.resizeObserver.observe(this.navbarRef.nativeElement);
    }

    this.updateScrolledState();
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
    this.sectionObserver?.disconnect();
  }

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.updateScrolledState();
    this.updateActiveSectionFromPosition();
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((isOpen) => !isOpen);
  }

  navigateToSection(event: Event, fragment: string): void {
    event.preventDefault();
    this.mobileMenuOpen.set(false);

    requestAnimationFrame(() => {
      this.updateNavMetrics();
      const target = document.getElementById(fragment);
      if (!target) return;

      const navHeight = this.navbarRef.nativeElement.offsetHeight;
      const top = target.getBoundingClientRect().top + window.scrollY - navHeight - 24;
      const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

      window.scrollTo({ top: Math.max(0, top), behavior: reducedMotion ? 'auto' : 'smooth' });
      history.replaceState(null, '', `#${fragment}`);
      this.activeSection.set(fragment);
    });
  }

  toggleFaq(index: number): void {
    this.openFaqIndex.update((current) => (current === index ? -1 : index));
  }

  private updateNavMetrics(): void {
    const navHeight = this.navbarRef.nativeElement.offsetHeight;
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
    this.isScrolled.set(window.scrollY > 8);
  }

  private updateActiveSectionFromPosition(): void {
    const marker = this.navbarRef.nativeElement.offsetHeight + 40;
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
