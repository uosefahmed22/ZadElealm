import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { SupportApiService } from '../../core/support/support-api.service';
import { SupportComponent } from './support.component';

describe('SupportComponent', () => {
  let supportApi: {
    createReport: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    supportApi = {
      createReport: vi.fn(() => of({ statusCode: 201, message: 'تم إرسال البلاغ بنجاح' })),
    };

    await TestBed.configureTestingModule({
      imports: [SupportComponent],
      providers: [{ provide: SupportApiService, useValue: supportApi }],
    }).compileComponents();
  });

  it('shows the create form without exposing report history', () => {
    const fixture = TestBed.createComponent(SupportComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('بلاغ جديد');
    expect(fixture.nativeElement.textContent).not.toContain('بلاغاتك السابقة');
  });

  it('keeps submit actionable and explains invalid fields after the click', () => {
    const fixture = TestBed.createComponent(SupportComponent);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    expect(button.disabled).toBe(false);
    button.click();
    fixture.detectChanges();

    expect(supportApi.createReport).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('اكتب عنوانًا مختصرًا');
    expect(fixture.nativeElement.textContent).toContain('اكتب تفاصيل المشكلة');
  });

  it('submits the real create-report contract then resets the form', () => {
    const fixture = TestBed.createComponent(SupportComponent);
    fixture.detectChanges();
    fixture.componentInstance.form.setValue({
      reportType: 'ProductIssue',
      titleOfTheIssue: '  خطأ في الاختبار  ',
      description: '  السؤال الثاني لا يعرض الاختيارات كاملة  ',
    });

    fixture.componentInstance.submit();
    fixture.detectChanges();

    expect(supportApi.createReport).toHaveBeenCalledWith({
      reportType: 'ProductIssue',
      titleOfTheIssue: 'خطأ في الاختبار',
      description: 'السؤال الثاني لا يعرض الاختيارات كاملة',
    });
    expect(fixture.componentInstance.form.getRawValue()).toEqual({
      reportType: 'Technical',
      titleOfTheIssue: '',
      description: '',
    });
    expect(fixture.nativeElement.textContent).toContain('تم الاستلام');
  });

  it('shows the normalized API error without clearing the report', () => {
    supportApi.createReport.mockReturnValue(throwError(() => new Error('تعذر الاتصال')));
    const fixture = TestBed.createComponent(SupportComponent);
    fixture.detectChanges();
    fixture.componentInstance.form.setValue({
      reportType: 'Other',
      titleOfTheIssue: 'اقتراح جديد',
      description: 'أريد اقتراح تحسين لتجربة عرض الدرس',
    });

    fixture.componentInstance.submit();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toContain('تعذر الاتصال');
    expect(fixture.componentInstance.form.controls.titleOfTheIssue.value).toBe('اقتراح جديد');
  });
});
