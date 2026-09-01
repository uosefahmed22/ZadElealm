import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { Subject, of } from 'rxjs';

import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { QuizResultDto } from '../../core/assessments/assessment.models';
import { QuizComponent } from './quiz.component';

describe('QuizComponent', () => {
  let api: { getQuiz: ReturnType<typeof vi.fn>; submitQuiz: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    api = { getQuiz: vi.fn(() => of({ statusCode: 200, data: quiz() })), submitQuiz: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [QuizComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ courseId: 10, quizId: 7 }) } } },
        { provide: AssessmentApiService, useValue: api },
      ],
    }).compileComponents();
  });

  it('enables submit only after every question and prevents a double submission', () => {
    const pending = new Subject<{ statusCode: number; data: QuizResultDto }>();
    api.submitQuiz.mockReturnValue(pending.asObservable());
    const fixture = TestBed.createComponent(QuizComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect(component.submitDisabled()).toBe(true);
    component.selectAnswer(1, 11);
    component.selectAnswer(2, 21);
    expect(component.submitDisabled()).toBe(false);

    component.submit();
    component.submit();
    expect(api.submitQuiz).toHaveBeenCalledOnce();
    expect(component.isSubmitting()).toBe(true);
  });

  it('renders successful and failed results at the sixty percent boundary', () => {
    const fixture = TestBed.createComponent(QuizComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.result.set(result(60, true));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('اجتزت الاختبار بنجاح');
    expect(fixture.nativeElement.textContent).toContain('عرض الشهادة');

    component.result.set(result(59, false));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('راجع الدروس');
    expect(fixture.nativeElement.textContent).toContain('إعادة الاختبار');
  });
});

function quiz() {
  return {
    id: 7,
    name: 'اختبار التجويد',
    description: 'اختبار ختامي',
    createdAt: '2026-01-01',
    course: { id: 10, name: 'التجويد' },
    questions: [
      { id: 1, text: 'السؤال الأول', quizId: 7, choices: [{ id: 11, text: 'اختيار' }] },
      { id: 2, text: 'السؤال الثاني', quizId: 7, choices: [{ id: 21, text: 'اختيار' }] },
    ],
  };
}

function result(score: number, isCompleted: boolean): QuizResultDto {
  return { quizName: 'اختبار التجويد', score, isCompleted, totalQuestions: 2, correctAnswers: isCompleted ? 2 : 1, unansweredQuestions: 0, date: '2026-01-01', questionResults: [] };
}
