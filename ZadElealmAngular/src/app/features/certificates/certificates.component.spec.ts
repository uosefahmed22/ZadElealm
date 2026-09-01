import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { CertificatesComponent } from './certificates.component';

describe('CertificatesComponent', () => {
  it('renders only certificate fields returned by the API and resolves the PDF link', () => {
    TestBed.configureTestingModule({
      imports: [CertificatesComponent],
      providers: [
        provideRouter([]),
        {
          provide: AssessmentApiService,
          useValue: {
            getCertificates: () =>
              of({
                statusCode: 200,
                data: [
                  {
                    id: 4,
                    name: 'شهادة إتمام',
                    description: 'اجتياز الاختبار',
                    pdfUrl: '/certificates/4.pdf',
                    completedDate: '2026-01-10',
                    userName: 'محمد أحمد',
                    quizName: 'اختبار التجويد',
                  },
                ],
              }),
          },
        },
      ],
    });
    const fixture = TestBed.createComponent(CertificatesComponent);
    fixture.detectChanges();
    const link = fixture.nativeElement.querySelector('a[href]') as HTMLAnchorElement;
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('اختبار التجويد');
    expect(text).toContain('محمد أحمد');
    expect(text).not.toContain('الدرجة');
    expect(text).not.toContain('الرقم المرجعي');
    expect(link.href).toBe('https://localhost:7054/certificates/4.pdf');
  });
});
