import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';

import { PlatformGuideComponent } from './platform-guide.component';

describe('PlatformGuideComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlatformGuideComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { data: { publicGuide: true } } } },
      ],
    }).compileComponents();
  });

  it('shows the complete journey while using only the two approved guide images', () => {
    const fixture = TestBed.createComponent(PlatformGuideComponent);
    fixture.detectChanges();

    const page = fixture.nativeElement as HTMLElement;
    const images = page.querySelectorAll('.visual-step img');

    expect(images).toHaveLength(2);
    expect(page.textContent).toContain('رحلتك من إنشاء الحساب إلى الشهادة');
    expect(page.textContent).toContain('ادخل اختبار المجال');
    expect(page.textContent).toContain('احصل على شهادتك');
    expect(page.textContent).toContain('التواصل مع الدعم');
    expect(page.querySelector('.guide-header')).not.toBeNull();
  });
});
