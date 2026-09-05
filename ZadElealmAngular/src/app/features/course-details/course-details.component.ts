import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
  OnDestroy,
  OnInit,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { catchError, forkJoin, of } from 'rxjs';

import { normalizeApiError } from '../../core/api/api-error.utils';
import { LearningApiService } from '../../core/learning/learning-api.service';
import {
  CourseDetailsDto,
  CourseProgressDto,
  CourseReplyDto,
  CourseReviewDto,
  completionPercentage,
  formatDuration,
  timeSpanToSeconds,
} from '../../core/learning/learning.models';
import {
  YOUTUBE_PLAYER_STATE,
  YoutubePlayerHandle,
  YoutubePlayerService,
  extractYoutubeVideoId,
} from '../../core/learning/youtube-player.service';

@Component({
  selector: 'app-course-details',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './course-details.component.html',
  styleUrl: './course-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CourseDetailsComponent implements OnInit, OnDestroy {
  @ViewChild('playerHost') private playerHost?: ElementRef<HTMLElement>;

  private readonly route = inject(ActivatedRoute);
  private readonly learningApi = inject(LearningApiService);
  private readonly youtube = inject(YoutubePlayerService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  readonly courseId = Number(this.route.snapshot.paramMap.get('courseId'));
  private player?: YoutubePlayerHandle;
  private progressTimer?: number;
  private destroyed = false;

  readonly course = signal<CourseDetailsDto | null>(null);
  readonly progress = signal<CourseProgressDto | null>(null);
  readonly activeIndex = signal(0);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly playerError = signal('');
  readonly trackingMessage = signal('');
  readonly isEnrolled = signal<boolean | null>(null);
  readonly isEnrolling = signal(false);
  readonly canRate = signal<boolean | null>(null);
  readonly selectedRating = signal(0);
  readonly isSubmittingRating = signal(false);
  readonly isSubmittingReview = signal(false);
  readonly feedbackMessage = signal('');
  readonly confirmingReviewId = signal<number | null>(null);
  readonly pendingReviewId = signal<number | null>(null);
  readonly expandedReviewIds = signal<ReadonlySet<number>>(new Set<number>());
  readonly repliesByReview = signal<Record<number, CourseReplyDto[]>>({});
  readonly replyDrafts = signal<Record<number, string>>({});
  readonly loadingRepliesId = signal<number | null>(null);
  readonly pendingReviewLikeId = signal<number | null>(null);
  readonly pendingReplyLikeId = signal<number | null>(null);
  readonly pendingReplyReviewId = signal<number | null>(null);
  readonly confirmingReplyId = signal<number | null>(null);
  readonly pendingDeleteReplyId = signal<number | null>(null);
  readonly hasOwnedReview = computed(
    () => this.course()?.review.some((review) => review.isOwnedByCurrentUser) ?? false,
  );
  readonly reviewForm = this.formBuilder.nonNullable.group({
    reviewText: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]],
  });
  readonly activeVideo = computed(() => this.course()?.videos[this.activeIndex()] ?? null);
  readonly completion = computed(() => {
    const server = this.progress();
    if (server) return Math.round(server.overallProgress);
    const videos = this.course()?.videos ?? [];
    return completionPercentage(videos.filter((video) => video.isCompleted).length, videos.length);
  });
  readonly quizUnlocked = computed(
    () => this.progress()?.isEligibleForQuiz ?? this.completion() >= 80,
  );

  readonly formatDuration = formatDuration;

  ngOnInit(): void {
    if (!Number.isInteger(this.courseId) || this.courseId <= 0) {
      this.isLoading.set(false);
      this.errorMessage.set('رابط الدورة غير صالح.');
      return;
    }
    this.loadCourse();
  }

  ngOnDestroy(): void {
    this.flushProgress();
    this.destroyed = true;
    this.stopTracking();
    this.player?.destroy();
  }

  @HostListener('window:pagehide')
  handlePageHide(): void {
    this.flushProgress();
  }

  selectVideo(index: number): void {
    const video = this.course()?.videos[index];
    if (!video || index === this.activeIndex() || this.isVideoLocked(index)) return;

    this.flushProgress();
    this.stopTracking();
    this.activeIndex.set(index);
    this.playerError.set('');
    const videoId = extractYoutubeVideoId(video.videoUrl);
    if (!videoId) {
      this.playerError.set('تعذر تحميل الفيديو');
      return;
    }

    if (this.player) {
      this.player.loadVideoById(videoId, timeSpanToSeconds(video.watchedDuration));
    } else {
      this.initializePlayer();
    }
  }

  retryPlayer(): void {
    this.playerError.set('');
    this.player?.destroy();
    this.player = undefined;
    this.initializePlayer();
  }

  retryCourse(): void {
    this.loadCourse();
  }

  isVideoLocked(index: number): boolean {
    const videos = this.course()?.videos ?? [];
    return index > 0 && videos.slice(0, index).some((video) => !video.isCompleted);
  }

  enroll(): void {
    if (this.isEnrolling() || this.isEnrolled()) return;
    this.isEnrolling.set(true);
    this.learningApi.enroll(this.courseId).subscribe({
      next: () => {
        this.isEnrolling.set(false);
        this.trackingMessage.set('تم تسجيلك في الدورة');
        this.loadCourse();
      },
      error: (error: unknown) => {
        this.isEnrolling.set(false);
        this.trackingMessage.set(normalizeApiError(error).message);
      },
    });
  }

  selectRating(value: number): void {
    if (this.canRate() !== true || this.isSubmittingRating()) return;
    this.selectedRating.set(value);
  }

  submitRating(): void {
    const value = this.selectedRating();
    if (this.isEnrolled() !== true || this.canRate() !== true || value < 1 || value > 5) {
      return;
    }

    this.isSubmittingRating.set(true);
    this.feedbackMessage.set('');
    this.learningApi
      .addRating(this.courseId, value)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmittingRating.set(false);
          this.canRate.set(false);
          this.feedbackMessage.set('تم حفظ تقييمك.');
          this.refreshFeedback();
        },
        error: (error: unknown) => {
          this.isSubmittingRating.set(false);
          this.feedbackMessage.set(normalizeApiError(error).message);
        },
      });
  }

  submitReview(): void {
    if (this.isEnrolled() !== true || this.hasOwnedReview() || this.reviewForm.invalid) {
      this.reviewForm.markAllAsTouched();
      return;
    }

    this.isSubmittingReview.set(true);
    this.feedbackMessage.set('');
    this.learningApi
      .addReview(this.courseId, this.reviewForm.controls.reviewText.value.trim())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmittingReview.set(false);
          this.reviewForm.reset();
          this.feedbackMessage.set('تم نشر مراجعتك.');
          this.refreshFeedback();
        },
        error: (error: unknown) => {
          this.isSubmittingReview.set(false);
          this.feedbackMessage.set(normalizeApiError(error).message);
        },
      });
  }

  requestDeleteReview(reviewId: number): void {
    this.confirmingReviewId.set(reviewId);
  }

  cancelDeleteReview(): void {
    this.confirmingReviewId.set(null);
  }

  confirmDeleteReview(review: CourseReviewDto): void {
    if (!review.isOwnedByCurrentUser || this.pendingReviewId() !== null) return;
    this.pendingReviewId.set(review.id);
    this.feedbackMessage.set('');
    this.learningApi
      .deleteReview(review.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.pendingReviewId.set(null);
          this.confirmingReviewId.set(null);
          this.course.update((course) =>
            course
              ? { ...course, review: course.review.filter((item) => item.id !== review.id) }
              : course,
          );
          this.feedbackMessage.set('تم حذف مراجعتك.');
        },
        error: (error: unknown) => {
          this.pendingReviewId.set(null);
          this.feedbackMessage.set(normalizeApiError(error).message);
        },
      });
  }

  toggleReviewLike(review: CourseReviewDto): void {
    if (this.isEnrolled() !== true || this.pendingReviewLikeId() !== null) return;
    this.pendingReviewLikeId.set(review.id);
    this.feedbackMessage.set('');
    this.learningApi
      .toggleReviewLike(review.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.pendingReviewLikeId.set(null);
          this.refreshFeedback();
        },
        error: (error: unknown) => {
          this.pendingReviewLikeId.set(null);
          this.feedbackMessage.set(normalizeApiError(error).message);
        },
      });
  }

  toggleReplies(reviewId: number): void {
    const expanded = new Set(this.expandedReviewIds());
    if (expanded.has(reviewId)) {
      expanded.delete(reviewId);
      this.expandedReviewIds.set(expanded);
      return;
    }

    expanded.add(reviewId);
    this.expandedReviewIds.set(expanded);
    this.loadReplies(reviewId);
  }

  setReplyDraft(reviewId: number, event: Event): void {
    const value = (event.target as HTMLTextAreaElement).value;
    this.replyDrafts.update((drafts) => ({ ...drafts, [reviewId]: value }));
  }

  replyDraft(reviewId: number): string {
    return this.replyDrafts()[reviewId] ?? '';
  }

  repliesFor(reviewId: number): CourseReplyDto[] {
    return this.repliesByReview()[reviewId] ?? [];
  }

  submitReply(reviewId: number): void {
    const replyText = (this.replyDrafts()[reviewId] ?? '').trim();
    if (
      this.isEnrolled() !== true ||
      this.pendingReplyReviewId() !== null ||
      replyText.length < 2 ||
      replyText.length > 500
    ) {
      return;
    }

    this.pendingReplyReviewId.set(reviewId);
    this.feedbackMessage.set('');
    this.learningApi
      .addReply(reviewId, replyText)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.pendingReplyReviewId.set(null);
          this.replyDrafts.update((drafts) => ({ ...drafts, [reviewId]: '' }));
          this.feedbackMessage.set('تم نشر ردك.');
          this.loadReplies(reviewId);
          this.refreshFeedback();
        },
        error: (error: unknown) => {
          this.pendingReplyReviewId.set(null);
          this.feedbackMessage.set(normalizeApiError(error).message);
        },
      });
  }

  toggleReplyLike(reviewId: number, reply: CourseReplyDto): void {
    if (this.isEnrolled() !== true || this.pendingReplyLikeId() !== null) return;
    this.pendingReplyLikeId.set(reply.id);
    this.feedbackMessage.set('');
    this.learningApi
      .toggleReplyLike(reply.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.pendingReplyLikeId.set(null);
          this.loadReplies(reviewId);
        },
        error: (error: unknown) => {
          this.pendingReplyLikeId.set(null);
          this.feedbackMessage.set(normalizeApiError(error).message);
        },
      });
  }

  requestDeleteReply(replyId: number): void {
    this.confirmingReplyId.set(replyId);
  }

  cancelDeleteReply(): void {
    this.confirmingReplyId.set(null);
  }

  confirmDeleteReply(reviewId: number, reply: CourseReplyDto): void {
    if (!reply.isOwnedByCurrentUser || this.pendingDeleteReplyId() !== null) return;
    this.pendingDeleteReplyId.set(reply.id);
    this.feedbackMessage.set('');
    this.learningApi
      .deleteReply(reply.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.pendingDeleteReplyId.set(null);
          this.confirmingReplyId.set(null);
          this.loadReplies(reviewId);
          this.refreshFeedback();
          this.feedbackMessage.set('تم حذف ردك.');
        },
        error: (error: unknown) => {
          this.pendingDeleteReplyId.set(null);
          this.feedbackMessage.set(normalizeApiError(error).message);
        },
      });
  }

  private loadCourse(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    forkJoin({
      course: this.learningApi.getCourse(this.courseId),
      progress: this.learningApi.getCourseProgress(this.courseId).pipe(catchError(() => of(null))),
      canRate: this.learningApi.canRate(this.courseId).pipe(catchError(() => of(null))),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ course, progress, canRate }) => {
          this.course.set(course.data);
          this.progress.set(progress);
          this.isEnrolled.set(course.data.isEnrolled);
          this.canRate.set(canRate?.data ?? null);
          const nextIncomplete = course.data.videos.findIndex((video) => !video.isCompleted);
          this.activeIndex.set(nextIncomplete >= 0 ? nextIncomplete : 0);
          this.isLoading.set(false);
          requestAnimationFrame(() => this.initializePlayer());
        },
        error: (error: unknown) => {
          this.isLoading.set(false);
          this.errorMessage.set(normalizeApiError(error).message);
        },
      });
  }

  private refreshFeedback(): void {
    this.learningApi
      .getCourse(this.courseId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.course.update((course) =>
            course
              ? {
                  ...course,
                  rating: response.data.rating,
                  review: response.data.review,
                }
              : response.data,
          );
        },
        error: () => this.feedbackMessage.set('تم الحفظ، لكن تعذر تحديث القائمة الآن.'),
      });
  }

  private loadReplies(reviewId: number): void {
    this.loadingRepliesId.set(reviewId);
    this.learningApi
      .getReplies(reviewId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.loadingRepliesId.set(null);
          this.repliesByReview.update((replies) => ({
            ...replies,
            [reviewId]: response.data,
          }));
        },
        error: (error: unknown) => {
          this.loadingRepliesId.set(null);
          this.feedbackMessage.set(normalizeApiError(error).message);
        },
      });
  }

  private initializePlayer(): void {
    if (this.destroyed || this.player || !this.playerHost || this.isEnrolled() !== true) return;
    const video = this.activeVideo();
    const videoId = video ? extractYoutubeVideoId(video.videoUrl) : null;
    if (!videoId) {
      if (video) this.playerError.set('تعذر تحميل الفيديو');
      return;
    }

    this.youtube
      .createPlayer(this.playerHost.nativeElement, videoId, {
        onReady: ({ target }) => {
          this.player = target;
          const watchedSeconds = timeSpanToSeconds(this.activeVideo()?.watchedDuration);
          if (watchedSeconds > 0) target.loadVideoById(videoId, watchedSeconds);
        },
        onStateChange: ({ data, target }) => {
          this.player = target;
          if (data === YOUTUBE_PLAYER_STATE.playing) this.startTracking();
          if (data === YOUTUBE_PLAYER_STATE.paused || data === YOUTUBE_PLAYER_STATE.ended) {
            this.stopTracking();
            this.flushProgress();
          }
        },
        onError: () => {
          this.stopTracking();
          this.playerError.set('تعذر تحميل الفيديو');
        },
      })
      .catch(() => this.playerError.set('تعذر تحميل الفيديو'));
  }

  private startTracking(): void {
    if (this.progressTimer !== undefined) return;
    this.progressTimer = window.setInterval(() => this.flushProgress(), 5_000);
  }

  private stopTracking(): void {
    if (this.progressTimer === undefined) return;
    window.clearInterval(this.progressTimer);
    this.progressTimer = undefined;
  }

  private flushProgress(): void {
    const player = this.player;
    const video = this.activeVideo();
    if (!player || !video) return;

    const duration = player.getDuration() || timeSpanToSeconds(video.videoDuration);
    const watchedSeconds = Math.min(
      Math.max(0, Math.floor(player.getCurrentTime())),
      Math.floor(duration),
    );
    if (watchedSeconds <= 0) return;

    this.learningApi.updateProgress(video.id, watchedSeconds).subscribe({
      next: (response) => {
        this.trackingMessage.set('تم حفظ تقدمك');
        if (response.data.isCompleted) {
          this.markVideoCompleted(video.id);
          this.refreshProgress();
        }
      },
      error: () => this.trackingMessage.set('تعذر حفظ التقدم الآن'),
    });
  }

  private markVideoCompleted(videoId: number): void {
    this.course.update((course) =>
      course
        ? {
            ...course,
            videos: course.videos.map((video) =>
              video.id === videoId ? { ...video, isCompleted: true } : video,
            ),
          }
        : course,
    );
    const videos = this.course()?.videos ?? [];
    const completedVideos = videos.filter((video) => video.isCompleted).length;
    const percentage = completionPercentage(completedVideos, videos.length);
    this.progress.update((progress) => ({
      ...(progress ?? {
        courseId: this.courseId,
        totalVideos: videos.length,
        remainingVideos: videos.length,
      }),
      videoProgress: percentage,
      overallProgress: percentage,
      completedVideos,
      totalVideos: videos.length,
      remainingVideos: videos.length - completedVideos,
      isEligibleForQuiz: percentage >= 80,
    }));
  }

  private refreshProgress(): void {
    this.learningApi
      .getCourseProgress(this.courseId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (progress) => this.progress.set(progress) });
  }
}
