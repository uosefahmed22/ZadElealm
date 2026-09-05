import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { CertificatesComponent } from './certificates.component';

describe('CertificatesComponent', () => {
  let assessmentApi: {
    getCertificates: ReturnType<typeof vi.fn>;
    downloadCertificate: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    assessmentApi = {
      getCertificates: vi.fn(() =>
        of({
          statusCode: 200,
          data: [certificate()],
        }),
      ),
      downloadCertificate: vi.fn(() => of(new Blob(['secure pdf'], { type: 'application/pdf' }))),
    };

    TestBed.configureTestingModule({
      imports: [CertificatesComponent],
      providers: [provideRouter([]), { provide: AssessmentApiService, useValue: assessmentApi }],
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  it('renders only certificate fields returned by the API', () => {
    const fixture = TestBed.createComponent(CertificatesComponent);
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('اختبار التجويد');
    expect(text).toContain('محمد أحمد');
    expect(text).not.toContain('الدرجة');
    expect(text).not.toContain('الرقم المرجعي');
    expect(text).toContain('عرض الشهادة PDF');
    expect(fixture.nativeElement.querySelector('a[href*="certificates"]')).toBeNull();
  });

  it('requests the protected PDF as a blob and opens only its local object URL', () => {
    const replace = vi.fn();
    const viewer = {
      close: vi.fn(),
      location: { replace },
      opener: window,
    };
    vi.spyOn(window, 'open').mockReturnValue(viewer as unknown as Window);

    const nativeUrl = URL;
    class TestUrl extends nativeUrl {
      static override createObjectURL = vi.fn(() => 'blob:http://localhost/secure-certificate');
      static override revokeObjectURL = vi.fn();
    }
    vi.stubGlobal('URL', TestUrl);

    const fixture = TestBed.createComponent(CertificatesComponent);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    button.click();
    fixture.detectChanges();

    expect(assessmentApi.downloadCertificate).toHaveBeenCalledWith(4);
    expect(replace).toHaveBeenCalledWith('blob:http://localhost/secure-certificate');
    expect(viewer.opener).toBeNull();
  });
});

function certificate() {
  return {
    id: 4,
    name: 'شهادة إتمام',
    description: 'اجتياز الاختبار',
    pdfUrl: '/api/Certificate/4/file',
    completedDate: '2026-01-10',
    userName: 'محمد أحمد',
    quizName: 'اختبار التجويد',
  };
}
