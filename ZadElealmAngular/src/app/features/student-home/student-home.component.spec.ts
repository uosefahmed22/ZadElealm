import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AssessmentApiService } from '../../core/assessments/assessment-api.service';
import { AchievementStateService } from '../../core/achievements/achievement-state.service';
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
  let achievementState: {
    dashboard: ReturnType<typeof signal>;
    errorMessage: ReturnType<typeof signal>;
    reload: ReturnType<typeof vi.fn>;
  };

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
    achievementState = {
      dashboard: signal(achievementDashboard()),
      errorMessage: signal(''),
      reload: vi.fn(() => of(achievementDashboard())),
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
        { provide: AchievementStateService, useValue: achievementState },
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
    expect(fixture.nativeElement.textContent).toContain('وصول سريع');
    expect(fixture.nativeElement.querySelectorAll('.quick-access__links a')).toHaveLength(4);
    expect(fixture.componentInstance.closestToCompletionCourseId()).toBe(1);
    expect(learningApi.getCourseProgress).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('إنجازاتي');
    expect(fixture.nativeElement.textContent).toContain('أسبوع من الهمة');
    expect(fixture.nativeElement.querySelectorAll('app-achievement-badge')).toHaveLength(2);
  });

  it('highlights the incomplete course that is closest to completion', () => {
    const courses = [course(1), course(2), course(3)];
    learningApi.getEnrolledCourses.mockReturnValue(
      of({
        statusCode: 200,
        data: {
          courses,
          progress: [progress(1, 25), progress(2, 80), progress(3, 100)],
          allEnrolledCourses: courses.length,
        },
      }),
    );

    const fixture = TestBed.createComponent(StudentHomeComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.closestToCompletionCourseId()).toBe(2);
    expect(fixture.nativeElement.querySelector('.completion-badge')?.textContent).toContain(
      'الأقرب للإنهاء',
    );
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

  it('keeps the learning dashboard available when achievements fail', () => {
    achievementState.dashboard.set(null);
    achievementState.errorMessage.set('achievements unavailable');

    const fixture = TestBed.createComponent(StudentHomeComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('الدورة 1');
    expect(fixture.nativeElement.textContent).toContain('تعذر تحميل إنجازاتك الآن');
    expect(fixture.nativeElement.textContent).toContain('achievements unavailable');
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
function progress(courseId: number, overallProgress = 50) {
  return {
    courseId,
    videoProgress: overallProgress,
    overallProgress,
    completedVideos: overallProgress >= 100 ? 2 : 1,
    totalVideos: 2,
    remainingVideos: 1,
    isEligibleForQuiz: false,
  };
}

function achievementDashboard() {
  return {
    currentStreak: 3,
    longestStreak: 5,
    unlockedCount: 1,
    totalCount: 11,
    newlyUnlocked: ['FirstLesson'],
    achievements: [
      {
        code: 'FirstLesson',
        title: 'البداية',
        description: 'أكمل أول درس.',
        iconKey: 'first-lesson',
        isUnlocked: true,
        unlockedAtUtc: '2026-09-08T10:00:00Z',
        currentValue: 1,
        target: 1,
      },
      {
        code: 'SevenDayStreak',
        title: 'أسبوع من الهمة',
        description: 'زر المنصة سبعة أيام متتالية.',
        iconKey: 'seven-day-streak',
        isUnlocked: false,
        unlockedAtUtc: null,
        currentValue: 5,
        target: 7,
      },
    ],
  };
}
