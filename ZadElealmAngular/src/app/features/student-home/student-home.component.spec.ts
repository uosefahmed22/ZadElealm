import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import { LearningApiService } from '../../core/learning/learning-api.service';
import { StudentHomeComponent } from './student-home.component';

describe('StudentHomeComponent', () => {
  let learningApi: {
    getEnrolledCourses: ReturnType<typeof vi.fn>;
    getCourseProgress: ReturnType<typeof vi.fn>;
    getFavorites: ReturnType<typeof vi.fn>;
  };
  let assessmentApi: { getCertificates: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    learningApi = {
      getEnrolledCourses: vi.fn(() =>
        of({
          statusCode: 200,
          data: { courses: [course(1)], progress: [progress(1)], allEnrolledCourses: 1 },
        }),
      ),
      getCourseProgress: vi.fn(() =>
        of({
          videoProgress: 50,
          overallProgress: 50,
          completedVideos: 1,
          totalVideos: 2,
          remainingVideos: 1,
          isEligibleForQuiz: false,
        }),
      ),
      getFavorites: vi.fn(() =>
        of({
          statusCode: 200,
          data: { courses: [course(2)], allFavoriteCourses: 1 },
        }),
      ),
    };
    assessmentApi = {
      getCertificates: vi.fn(() => of({ statusCode: 200, data: [certificate()] })),
    };
    await TestBed.configureTestingModule({
      imports: [StudentHomeComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthSessionService,
          useValue: { user: signal({ displayName: 'محمد', email: 'm@test.com' }) },
        },
        { provide: LearningApiService, useValue: learningApi },
        { provide: AssessmentApiService, useValue: assessmentApi },
      ],
    }).compileComponents();
  });

  it('renders real dashboard data and derived counts', () => {
    const fixture = TestBed.createComponent(StudentHomeComponent);
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('أهلًا بك، محمد');
    expect(text).toContain('الدورة 1');
    expect(text).toContain('اختبار التجويد');
    expect(text).toContain('شهادة اجتياز');
    expect(text).toContain('عرض الشهادة');
    expect(text).toContain('الدورة 2');
    expect(fixture.componentInstance.ongoingCount()).toBe(1);
    expect(fixture.componentInstance.certificates()).toHaveLength(1);
    expect(learningApi.getCourseProgress).not.toHaveBeenCalled();
  });

  it('renders empty states without invented statistics', () => {
    learningApi.getEnrolledCourses.mockReturnValue(
      of({
        statusCode: 200,
        message: 'لا توجد دورات مسجلة',
        data: { courses: [], progress: [], allEnrolledCourses: 0 },
      }),
    );
    assessmentApi.getCertificates.mockReturnValue(of({ statusCode: 200, data: [] }));
    learningApi.getFavorites.mockReturnValue(
      of({ statusCode: 200, data: { courses: [], allFavoriteCourses: 0 } }),
    );
    const fixture = TestBed.createComponent(StudentHomeComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('لم تسجل في أي دورة بعد');
    expect(fixture.nativeElement.textContent).toContain('لم تحصل على شهادات بعد');
    expect(fixture.nativeElement.textContent).toContain('لم تضف أي دورة إلى المفضلة بعد');
  });

  it('shows only three favorite courses and links to the full favorites page', () => {
    learningApi.getFavorites.mockReturnValue(
      of({
        statusCode: 200,
        data: {
          courses: [course(2), course(3), course(4), course(5)],
          allFavoriteCourses: 4,
        },
      }),
    );

    const fixture = TestBed.createComponent(StudentHomeComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.favorite-grid app-course-card')).toHaveLength(
      3,
    );
    expect(fixture.nativeElement.textContent).toContain('عرض الكل');
    expect(fixture.nativeElement.textContent).not.toContain('الدورة 5');
  });

  it('shows only three recent courses and links to the full library', () => {
    const courses = [course(1), course(2), course(3), course(4)];
    learningApi.getEnrolledCourses.mockReturnValue(
      of({
        statusCode: 200,
        data: {
          courses,
          progress: courses.map((item) => progress(item.id)),
          allEnrolledCourses: courses.length,
        },
      }),
    );

    const fixture = TestBed.createComponent(StudentHomeComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.learning-card')).toHaveLength(3);
    expect(fixture.nativeElement.textContent).toContain('عرض كل دوراتي');
    expect(fixture.nativeElement.textContent).not.toContain('الدورة 4');
  });

  it('renders an actionable error state', () => {
    learningApi.getEnrolledCourses.mockReturnValue(
      throwError(() => new Error('dashboard unavailable')),
    );
    const fixture = TestBed.createComponent(StudentHomeComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('dashboard unavailable');
    expect(fixture.nativeElement.textContent).toContain('إعادة المحاولة');
  });
});

function course(id: number) {
  return {
    id,
    name: `الدورة ${id}`,
    description: 'وصف',
    author: 'الشيخ أحمد',
    courseLanguage: 'العربية',
    courseVideosCount: 2,
    rating: 4,
    imageUrl: '',
    category: { id: 1, name: 'القرآن', description: '', imageUrl: '' },
    createdAt: '2026-01-01',
  };
}
function certificate() {
  return {
    id: 4,
    name: 'شهادة',
    description: 'إتمام',
    pdfUrl: '/cert.pdf',
    completedDate: '2026-01-01',
    userName: 'محمد',
    quizName: 'اختبار التجويد',
  };
}
function progress(courseId: number) {
  return {
    courseId,
    videoProgress: 50,
    overallProgress: 50,
    completedVideos: 1,
    totalVideos: 2,
    remainingVideos: 1,
    isEligibleForQuiz: false,
  };
}
