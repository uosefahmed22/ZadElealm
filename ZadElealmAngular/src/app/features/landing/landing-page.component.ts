import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface NavItem {
  readonly label: string;
  readonly fragment: string;
}

interface FeatureCard {
  readonly title: string;
  readonly description: string;
  readonly icon: 'courses' | 'progress' | 'quiz' | 'certificate';
}

interface JourneyStep {
  readonly number: string;
  readonly title: string;
  readonly description: string;
  readonly icon: 'account' | 'book' | 'screen' | 'award';
}

interface CoursePath {
  readonly level: string;
  readonly title: string;
  readonly description: string;
  readonly accent: 'green' | 'sand' | 'olive';
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
export class LandingPageComponent {
  readonly navItems: readonly NavItem[] = [
    { label: 'الرئيسية', fragment: 'top' },
    { label: 'المميزات', fragment: 'features' },
    { label: 'كيف تبدأ', fragment: 'journey' },
    { label: 'الأسئلة الشائعة', fragment: 'faq' },
  ];

  readonly featureCards: readonly FeatureCard[] = [
    {
      title: 'دورات منظمة',
      description: 'محتوى مرتب يساعدك على التدرج من الأساس إلى التطبيق بثبات ووضوح.',
      icon: 'courses',
    },
    {
      title: 'متابعة التقدم',
      description: 'تشاهد مسارك خطوة بخطوة وتعرف ما الذي أنجزته وما الذي ينتظرك بعد ذلك.',
      icon: 'progress',
    },
    {
      title: 'اختبارات تفاعلية',
      description: 'اختبر فهمك بعد الدروس وحافظ على ثبات التعلم قبل الانتقال للمرحلة التالية.',
      icon: 'quiz',
    },
    {
      title: 'شهادات إتمام',
      description: 'احفظ إنجازك في نهاية المسار بشهادة تساعدك على توثيق تقدمك العلمي.',
      icon: 'certificate',
    },
  ];

  readonly journeySteps: readonly JourneyStep[] = [
    {
      number: '1',
      title: 'سجل حسابك',
      description: 'أنشئ حساب الطالب وابدأ من واجهة عربية واضحة ومريحة.',
      icon: 'account',
    },
    {
      number: '2',
      title: 'اختر مسارك',
      description: 'ابدأ بالدورات المناسبة لمستواك والهدف الذي تريد الوصول إليه.',
      icon: 'book',
    },
    {
      number: '3',
      title: 'تعلّم وتدرّب',
      description: 'شاهد الدروس وتابع تقدمك وحل الاختبارات داخل المسار نفسه.',
      icon: 'screen',
    },
    {
      number: '4',
      title: 'أكمل واحصل على شهادتك',
      description: 'عند إتمام المسار ترى إنجازك بوضوح وتحفظ شهادتك التعليمية.',
      icon: 'award',
    },
  ];

  readonly coursePaths: readonly CoursePath[] = [
    {
      level: 'مبتدئ',
      title: 'أحكام التلاوة',
      description: 'مثال لمسار منظم يبدأ من القواعد الأساسية وينتقل بك إلى قراءة أكثر صحة.',
      accent: 'green',
    },
    {
      level: 'مبتدئ',
      title: 'الفقه الميسر',
      description: 'مثال لمسار تدريجي يشرح الأساسيات بلغة سهلة مع ترتيب واضح للدروس.',
      accent: 'sand',
    },
    {
      level: 'متوسط',
      title: 'السيرة النبوية',
      description: 'مثال لمسار يربط بين التعلم والمعنى التربوي في رحلة متسلسلة ومريحة.',
      accent: 'olive',
    },
  ];

  readonly progressChecklist = [
    'استكمال الدروس المنظمة',
    'حل الاختبارات التفاعلية',
    'متابعة التقدم داخل المسار',
    'الحصول على الشهادة بعد الإنجاز',
  ];

  readonly faqItems: readonly FaqItem[] = [
    {
      question: 'كيف أبدأ التعلم في المنصة؟',
      answer:
        'تبدأ بإنشاء حسابك، ثم تختار المسار المناسب لك، وبعدها تتابع الدروس والاختبارات من مكان واحد.',
    },
    {
      question: 'هل الدورات تشمل شهادات؟',
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

  toggleFaq(index: number): void {
    this.openFaqIndex.update((current) => (current === index ? -1 : index));
  }
}
