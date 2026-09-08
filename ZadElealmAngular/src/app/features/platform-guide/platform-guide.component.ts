import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ArabicNumberPipe } from '../../shared/pipes/arabic-number.pipe';

interface GuideStep {
  readonly number: number;
  readonly title: string;
  readonly description: string;
  readonly icon: string;
}

@Component({
  selector: 'app-platform-guide',
  imports: [RouterLink, ArabicNumberPipe],
  templateUrl: './platform-guide.component.html',
  styleUrl: './platform-guide.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlatformGuideComponent {
  private readonly route = inject(ActivatedRoute);

  readonly showPublicHeader = this.route.snapshot.data['publicGuide'] === true;

  readonly remainingSteps: readonly GuideStep[] = [
    {
      number: 3,
      title: 'تعلّم وتابع تقدمك',
      description:
        'شاهد دروس الدورة بالترتيب. تحفظ المنصة تقدمك تلقائيًا، ويمكنك الرجوع من «دوراتي» وإكمال ما توقفت عنده.',
      icon: 'play-screen',
    },
    {
      number: 4,
      title: 'ادخل اختبار المجال',
      description:
        'بعد إتمام دورة مؤهلة يظهر لك اختبار المجال، مثل اختبار الفقه. الاختبار له وقت محدد ويتم تسليم إجاباتك مرة واحدة.',
      icon: 'clipboard',
    },
    {
      number: 5,
      title: 'راجع نتيجتك',
      description:
        'بعد التسليم تعرف درجتك، وتستطيع مراجعة إجاباتك ومعرفة الإجابات الصحيحة والنقاط التي تحتاج إلى تحسينها.',
      icon: 'trend',
    },
    {
      number: 6,
      title: 'احصل على شهادتك',
      description:
        'عند اجتياز الاختبار تُضاف الشهادة إلى صفحة «شهاداتي»، ويمكنك فتحها أو تحميلها والرجوع إلى مراجعة اختبارك.',
      icon: 'award',
    },
  ];
}
