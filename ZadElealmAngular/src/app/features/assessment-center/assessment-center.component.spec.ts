import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { AssessmentCenterComponent } from './assessment-center.component';

describe('AssessmentCenterComponent', () => {
  it('renders one public assessment without exposing internal forms', async () => {
    await TestBed.configureTestingModule({
      imports: [AssessmentCenterComponent],
      providers: [
        provideRouter([]),
        {
          provide: AssessmentApiService,
          useValue: {
            getAssessments: () => of({
              statusCode: 200,
              data: [{ id: 3, name: 'اختبار الفقه', description: 'اختبار عام', categoryName: 'الفقه', passingScore: 60, isEligible: true, isCompleted: false, bestScore: null }],
            }),
          },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(AssessmentCenterComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('اختبار الفقه');
    expect(fixture.nativeElement.textContent).not.toContain('FIQH-A');
    expect(fixture.nativeElement.querySelectorAll('.assessment-card')).toHaveLength(1);
  });
});
