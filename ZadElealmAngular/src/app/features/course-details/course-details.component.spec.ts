import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LearningApiService } from '../../core/learning/learning-api.service';
import {
  YoutubePlayerHandle,
  YoutubePlayerService,
} from '../../core/learning/youtube-player.service';
import { CourseDetailsComponent } from './course-details.component';

describe('CourseDetailsComponent', () => {
  let learningApi: {
    getCourse: ReturnType<typeof vi.fn>;
    getCourseProgress: ReturnType<typeof vi.fn>;
    updateProgress: ReturnType<typeof vi.fn>;
    getEnrolledCourses: ReturnType<typeof vi.fn>;
    enroll: ReturnType<typeof vi.fn>;
    canRate: ReturnType<typeof vi.fn>;
    addRating: ReturnType<typeof vi.fn>;
    addReview: ReturnType<typeof vi.fn>;
    deleteReview: ReturnType<typeof vi.fn>;
    toggleReviewLike: ReturnType<typeof vi.fn>;
    getReplies: ReturnType<typeof vi.fn>;
    addReply: ReturnType<typeof vi.fn>;
    toggleReplyLike: ReturnType<typeof vi.fn>;
    deleteReply: ReturnType<typeof vi.fn>;
  };
  let youtube: { createPlayer: ReturnType<typeof vi.fn> };
  let player: YoutubePlayerHandle;
  let currentTime: number;

  beforeEach(async () => {
    currentTime = 0;
    player = {
      destroy: vi.fn(),
      getCurrentTime: vi.fn(() => currentTime),
      getDuration: vi.fn(() => 100),
      loadVideoById: vi.fn(),
      pauseVideo: vi.fn(),
    };
    learningApi = {
      getCourse: vi.fn(() => of({ statusCode: 200, data: courseDetails() })),
      getCourseProgress: vi.fn(() => of(progress())),
      getEnrolledCourses: vi.fn(() =>
        of({ statusCode: 200, data: { courses: [{ id: 10 }], allEnrolledCourses: 1 } }),
      ),
      enroll: vi.fn(() => of({ statusCode: 200 })),
      canRate: vi.fn(() => of({ statusCode: 200, data: true })),
      addRating: vi.fn(() => of({ statusCode: 200 })),
      addReview: vi.fn(() => of({ statusCode: 200 })),
      deleteReview: vi.fn(() => of({ statusCode: 200 })),
      toggleReviewLike: vi.fn(() => of({ statusCode: 200 })),
      getReplies: vi.fn(() => of({ statusCode: 200, data: [] })),
      addReply: vi.fn(() => of({ statusCode: 200 })),
      toggleReplyLike: vi.fn(() => of({ statusCode: 200 })),
      deleteReply: vi.fn(() => of({ statusCode: 200 })),
      updateProgress: vi.fn(() =>
        of({
          statusCode: 200,
          data: { videoId: 2, courseId: 10, watchedDuration: 85, isCompleted: true },
        }),
      ),
    };
    youtube = {
      createPlayer: vi.fn((_element, _videoId, events) => {
        events.onReady({ target: player });
        return Promise.resolve(player);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [CourseDetailsComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ courseId: 10 }) } },
        },
        { provide: LearningApiService, useValue: learningApi },
        { provide: YoutubePlayerService, useValue: youtube },
      ],
    }).compileComponents();
    vi.spyOn(window, 'requestAnimationFrame').mockImplementation(() => 1);
  });

  afterEach(() => vi.restoreAllMocks());

  it('uses server progress to unlock the quiz at eighty percent', () => {
    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.completion()).toBe(80);
    expect(fixture.componentInstance.quizUnlocked()).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('الانتقال للاختبار');
    expect(fixture.nativeElement.querySelector('.course-progress-card a')).not.toBeNull();
  });

  it('switches lessons through the mocked YouTube player without reloading', () => {
    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    (component as unknown as { initializePlayer(): void }).initializePlayer();

    component.selectVideo(2);

    expect(component.activeIndex()).toBe(2);
    expect(player.loadVideoById).toHaveBeenCalledWith('video003', 85);
  });

  it('keeps later lessons locked until every previous lesson is completed', () => {
    const lockedCourse = courseDetails();
    lockedCourse.videos = lockedCourse.videos.map((video) => ({
      ...video,
      isCompleted: false,
      watchedDuration: '00:00:00',
    }));
    learningApi.getCourse.mockReturnValue(of({ statusCode: 200, data: lockedCourse }));
    learningApi.getCourseProgress.mockReturnValue(
      of({
        ...progress(),
        videoProgress: 0,
        overallProgress: 0,
        completedVideos: 0,
        isEligibleForQuiz: false,
      }),
    );

    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.selectVideo(1);

    expect(component.activeIndex()).toBe(0);
    expect(fixture.nativeElement.querySelectorAll('.lesson-list button')[1].disabled).toBe(true);
  });

  it('reloads protected video links after a successful enrollment', () => {
    const previewCourse = {
      ...courseDetails(),
      isEnrolled: false,
      videos: courseDetails().videos.map((video) => ({ ...video, videoUrl: '' })),
    };
    learningApi.getCourse
      .mockReturnValueOnce(of({ statusCode: 200, data: previewCourse }))
      .mockReturnValueOnce(of({ statusCode: 200, data: courseDetails() }));

    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.enroll();

    expect(learningApi.enroll).toHaveBeenCalledWith(10);
    expect(learningApi.getCourse).toHaveBeenCalledTimes(2);
    expect(component.isEnrolled()).toBe(true);
    expect(component.activeVideo()?.videoUrl).not.toBe('');
  });

  it('sends watched seconds and completes a video at eighty-five percent', () => {
    vi.useFakeTimers();
    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    (component as unknown as { initializePlayer(): void }).initializePlayer();
    currentTime = 85;

    (component as unknown as { startTracking(): void }).startTracking();
    vi.advanceTimersByTime(5_000);

    expect(learningApi.updateProgress).toHaveBeenCalledWith(5, 85);
    expect(component.course()?.videos[4].isCompleted).toBe(true);
    vi.useRealTimers();
  });

  it('does not unlock a lesson when the server rejects saving its progress', () => {
    const pendingCourse = courseDetails();
    pendingCourse.videos[4] = {
      ...pendingCourse.videos[4],
      isCompleted: false,
      watchedDuration: '00:00:00',
    };
    learningApi.getCourse.mockReturnValue(of({ statusCode: 200, data: pendingCourse }));
    learningApi.updateProgress.mockReturnValue(
      throwError(() => ({ status: 403, error: { message: 'أكمل الدروس السابقة' } })),
    );

    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    (component as unknown as { initializePlayer(): void }).initializePlayer();
    currentTime = 85;

    (component as unknown as { flushProgress(): void }).flushProgress();

    expect(component.course()?.videos[4].isCompleted).toBe(false);
    expect(component.trackingMessage()).toBe('تعذر حفظ التقدم الآن');
  });

  it('submits one rating and disables the rating form after success', () => {
    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.selectRating(5);
    component.submitRating();

    expect(learningApi.addRating).toHaveBeenCalledWith(10, 5);
    expect(component.canRate()).toBe(false);
    expect(component.feedbackMessage()).toContain('تم حفظ تقييمك');
  });

  it('validates and submits a review then refreshes the real review list', () => {
    const withReview = courseDetails();
    withReview.review = [review()];
    learningApi.getCourse
      .mockReturnValueOnce(of({ statusCode: 200, data: courseDetails() }))
      .mockReturnValueOnce(of({ statusCode: 200, data: withReview }));
    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.reviewForm.controls.reviewText.setValue('دورة نافعة وشرحها واضح');
    component.submitReview();

    expect(learningApi.addReview).toHaveBeenCalledWith(10, 'دورة نافعة وشرحها واضح');
    expect(component.course()?.review).toHaveLength(1);
    expect(component.hasOwnedReview()).toBe(true);
  });

  it('deletes only an owned review after explicit confirmation', () => {
    const withReview = courseDetails();
    withReview.review = [review()];
    learningApi.getCourse.mockReturnValue(of({ statusCode: 200, data: withReview }));
    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.requestDeleteReview(15);
    component.confirmDeleteReview(component.course()!.review[0]);

    expect(learningApi.deleteReview).toHaveBeenCalledWith(15);
    expect(component.course()?.review).toHaveLength(0);
  });

  it('toggles a review like and refreshes its real count and ownership state', () => {
    const initial = courseDetails();
    initial.review = [review()];
    const liked = courseDetails();
    liked.review = [{ ...review(), likesCount: 1, isLikedByCurrentUser: true }];
    learningApi.getCourse
      .mockReturnValueOnce(of({ statusCode: 200, data: initial }))
      .mockReturnValueOnce(of({ statusCode: 200, data: liked }));
    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.toggleReviewLike(component.course()!.review[0]);

    expect(learningApi.toggleReviewLike).toHaveBeenCalledWith(15);
    expect(component.course()?.review[0].likesCount).toBe(1);
    expect(component.course()?.review[0].isLikedByCurrentUser).toBe(true);
  });

  it('loads replies and publishes a valid reply through the real reply contract', () => {
    const initial = courseDetails();
    initial.review = [review()];
    learningApi.getCourse.mockReturnValue(of({ statusCode: 200, data: initial }));
    learningApi.getReplies
      .mockReturnValueOnce(of({ statusCode: 200, data: [] }))
      .mockReturnValueOnce(of({ statusCode: 200, data: [reply()] }));
    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.toggleReplies(15);
    component.replyDrafts.set({ 15: 'رد واضح ومفيد' });
    component.submitReply(15);

    expect(learningApi.addReply).toHaveBeenCalledWith(15, 'رد واضح ومفيد');
    expect(component.repliesByReview()[15]).toEqual([reply()]);
    expect(component.replyDrafts()[15]).toBe('');
  });

  it('likes and deletes only an owned reply after confirmation', () => {
    const initial = courseDetails();
    initial.review = [review()];
    learningApi.getCourse.mockReturnValue(of({ statusCode: 200, data: initial }));
    learningApi.getReplies.mockReturnValue(
      of({
        statusCode: 200,
        data: [{ ...reply(), replyLikesCount: 1, isLikedByCurrentUser: true }],
      }),
    );
    const fixture = TestBed.createComponent(CourseDetailsComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const ownedReply = reply();

    component.toggleReplyLike(15, ownedReply);
    component.requestDeleteReply(ownedReply.id);
    component.confirmDeleteReply(15, ownedReply);

    expect(learningApi.toggleReplyLike).toHaveBeenCalledWith(24);
    expect(learningApi.deleteReply).toHaveBeenCalledWith(24);
    expect(component.confirmingReplyId()).toBeNull();
  });
});

function courseDetails() {
  return {
    name: 'أساسيات التجويد',
    description: 'شرح مبسط',
    author: 'الشيخ أحمد',
    rating: 4.8,
    courseLanguage: 'العربية',
    courseVideosCount: 5,
    imageUrl: 'course.jpg',
    createdAt: '2026-01-01',
    totalEnrolledStudents: 12,
    isEnrolled: true,
    category: { id: 1, name: 'القرآن', description: '', imageUrl: '' },
    videos: [1, 2, 3, 4, 5].map((id) => ({
      id,
      title: `الدرس ${id}`,
      description: 'وصف الدرس',
      videoUrl: `https://youtu.be/video00${id}`,
      thumbnailUrl: '',
      videoDuration: '00:01:40',
      isCompleted: id <= 4,
      watchedDuration: id <= 4 ? '00:01:25' : '00:00:00',
    })),
    review: [] as ReturnType<typeof review>[],
    quizzes: [{ id: 7, name: 'اختبار الدورة', description: '', createdAt: '2026-01-01' }],
  };
}

function review() {
  return {
    id: 15,
    text: 'دورة نافعة وشرحها واضح',
    courseId: 10,
    createdAt: '2026-01-02',
    appUserId: 'student-1',
    displayName: 'محمد أحمد',
    imageUrl: '',
    hasReplies: false,
    repliesCount: 0,
    likesCount: 0,
    isOwnedByCurrentUser: true,
    isLikedByCurrentUser: false,
  };
}

function reply() {
  return {
    id: 24,
    text: 'رد واضح ومفيد',
    appUserId: 'student-1',
    displayName: 'محمد أحمد',
    userImage: '',
    createdAt: '2026-01-03',
    replyLikesCount: 0,
    isOwnedByCurrentUser: true,
    isLikedByCurrentUser: false,
  };
}

function progress() {
  return {
    videoProgress: 80,
    overallProgress: 80,
    completedVideos: 4,
    totalVideos: 5,
    remainingVideos: 1,
    isEligibleForQuiz: true,
  };
}
