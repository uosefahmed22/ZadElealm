import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { AssessmentExamComponent } from './assessment-exam.component';

describe('AssessmentExamComponent', () => {
  it('submits the public assessment id without receiving an internal form id', async () => {
    const api = {
      getAssessment: vi.fn(() => of({ statusCode: 200, data: {
        id: 8,
        name: 'اختبار الفقه',
        description: 'اختبار عام',
        passingScore: 60,
        durationMinutes: 30,
        attemptStartedAtUtc: new Date(Date.now() - 1_000).toISOString(),
        attemptExpiresAtUtc: new Date(Date.now() + 30 * 60_000).toISOString(),
        questions: [{ id: 11, text: 'سؤال', choices: [{ id: 21, text: 'اختيار' }] }],
      } })),
      submitAssessment: vi.fn(() => of({ statusCode: 200, data: {
        assessmentName: 'اختبار الفقه', score: 100, isCompleted: true,
        totalQuestions: 1, correctAnswers: 1, date: '2026-09-05',
      } })),
    };
    await TestBed.configureTestingModule({
      imports: [AssessmentExamComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ assessmentId: 8 }) } } },
        { provide: AssessmentApiService, useValue: api },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(AssessmentExamComponent);
    fixture.detectChanges();
    fixture.componentInstance.selectAnswer(11, 21);
    const submitEvent = { preventDefault: vi.fn() } as unknown as SubmitEvent;
    fixture.componentInstance.requestSubmit(submitEvent);

    expect(submitEvent.preventDefault).toHaveBeenCalled();
    expect(fixture.componentInstance.showSubmitConfirmation()).toBe(true);
    fixture.componentInstance.confirmSubmit();

    expect(api.submitAssessment).toHaveBeenCalledWith(8, {
      studentAnswers: [{ questionId: 11, choiceId: 21 }],
    });
    expect(fixture.componentInstance.result()?.isCompleted).toBe(true);
    fixture.destroy();
  });
});
