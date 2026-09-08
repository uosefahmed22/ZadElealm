import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { LearningApiService } from '../../core/learning/learning-api.service';
import { MyCoursesComponent } from './my-courses.component';

describe('MyCoursesComponent', () => {
  let learningApi: {
    getEnrolledCourses: ReturnType<typeof vi.fn>;
    getCourseProgress: ReturnType<typeof vi.fn>;
    enroll: ReturnType<typeof vi.fn>;
    unenroll: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    learningApi = {
      getEnrolledCourses: vi.fn(() =>
        of({
          statusCode: 200,
          data: { courses: [course()], progress: [progress()], allEnrolledCourses: 1 },
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
      enroll: vi.fn(() => of({ statusCode: 200 })),
      unenroll: vi.fn(() => of({ statusCode: 200 })),
    };

    await TestBed.configureTestingModule({
      imports: [MyCoursesComponent],
      providers: [provideRouter([]), { provide: LearningApiService, useValue: learningApi }],
    }).compileComponents();
  });

  it('renders enrolled courses with backend progress', () => {
    const fixture = TestBed.createComponent(MyCoursesComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('أساسيات التجويد');
    expect(fixture.nativeElement.textContent).toContain('٥٠٪');
    expect(learningApi.getCourseProgress).not.toHaveBeenCalled();
  });

  it('requires confirmation and can undo without losing the course progress', () => {
    const fixture = TestBed.createComponent(MyCoursesComponent);
    fixture.detectChanges();

    fixture.componentInstance.requestUnenroll(10);
    fixture.detectChanges();
    fixture.componentInstance.confirmUnenroll(fixture.componentInstance.courses()[0]);
    fixture.detectChanges();

    expect(learningApi.unenroll).toHaveBeenCalledWith(10);
    expect(fixture.componentInstance.courses()).toHaveLength(0);
    expect(fixture.nativeElement.textContent).toContain('تم إلغاء التسجيل');
    expect(fixture.nativeElement.textContent).toContain('تقدمك في الدورة محفوظ');

    fixture.componentInstance.undoUnenroll();
    fixture.detectChanges();

    expect(learningApi.enroll).toHaveBeenCalledWith(10);
    expect(fixture.componentInstance.courses()).toHaveLength(1);
    expect(fixture.componentInstance.courses()[0].progress?.overallProgress).toBe(50);
    expect(fixture.nativeElement.textContent).toContain('تمت استعادة');
    fixture.destroy();
  });
});

function course() {
  return {
    id: 10,
    name: 'أساسيات التجويد',
    description: 'وصف',
    author: 'أحمد محمود',
    courseLanguage: 'العربية',
    courseVideosCount: 2,
    rating: 5,
    imageUrl: 'course.jpg',
    category: { id: 1, name: 'القرآن', description: '', imageUrl: '' },
    createdAt: '2026-01-01',
  };
}

function progress() {
  return {
    courseId: 10,
    videoProgress: 50,
    overallProgress: 50,
    completedVideos: 1,
    totalVideos: 2,
    remainingVideos: 1,
    isEligibleForQuiz: false,
  };
}
