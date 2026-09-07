import { CategoryDto, CourseDto } from '../catalog/catalog.models';

export const COURSE_EXAM_REQUIRED_COMPLETION_PERCENTAGE = 100;

export interface CourseVideoDto {
  id: number;
  title: string;
  description: string;
  videoUrl: string;
  thumbnailUrl: string;
  videoDuration: string;
  isCompleted: boolean;
  watchedDuration: string;
}

export interface CourseQuizSummaryDto {
  id: number;
  name: string;
  description: string;
  createdAt: string;
}

export interface CourseReviewDto {
  id: number;
  text: string;
  courseId: number;
  createdAt: string;
  appUserId: string;
  displayName: string;
  imageUrl: string;
  hasReplies: boolean;
  repliesCount: number;
  likesCount: number;
  isOwnedByCurrentUser: boolean;
  isLikedByCurrentUser: boolean;
}

export interface CourseReplyDto {
  id: number;
  text: string;
  appUserId: string;
  displayName: string;
  userImage: string;
  createdAt: string;
  replyLikesCount: number;
  isOwnedByCurrentUser: boolean;
  isLikedByCurrentUser: boolean;
}

export interface CourseDetailsDto {
  name: string;
  description: string;
  author: string;
  rating: number;
  courseLanguage: string;
  courseVideosCount: number;
  imageUrl: string;
  createdAt: string;
  totalEnrolledStudents: number;
  isEnrolled: boolean;
  category: CategoryDto;
  videos: CourseVideoDto[];
  review: CourseReviewDto[];
  quizzes: CourseQuizSummaryDto[];
}

export interface CourseProgressDto {
  courseId: number;
  videoProgress: number;
  overallProgress: number;
  completedVideos: number;
  totalVideos: number;
  remainingVideos: number;
  isEligibleForQuiz: boolean;
}

export interface EligibilityResponse {
  isEligible: boolean;
  message: string;
}

export interface VideoProgressDto {
  videoId: number;
  courseId: number;
  watchedDuration: number;
  isCompleted: boolean;
}

export interface EnrolledCoursesData {
  courses: CourseDto[];
  progress: CourseProgressDto[];
  allEnrolledCourses: number;
}

export interface FavoriteCoursesData {
  courses: CourseDto[];
  allFavoriteCourses: number;
}

export function timeSpanToSeconds(value: string | number | null | undefined): number {
  if (typeof value === 'number') return Number.isFinite(value) ? value : 0;
  if (!value) return 0;
  const parts = value.split(':').map(Number);
  if (parts.some((part) => !Number.isFinite(part))) return 0;
  if (parts.length === 3) return parts[0] * 3600 + parts[1] * 60 + parts[2];
  if (parts.length === 2) return parts[0] * 60 + parts[1];
  return parts[0] ?? 0;
}

export function formatDuration(value: string | number): string {
  const total = Math.max(0, Math.floor(timeSpanToSeconds(value)));
  const hours = Math.floor(total / 3600);
  const minutes = Math.floor((total % 3600) / 60);
  const seconds = total % 60;
  const padded = (part: number) => String(part).padStart(2, '0');
  return hours > 0
    ? `${hours}:${padded(minutes)}:${padded(seconds)}`
    : `${minutes}:${padded(seconds)}`;
}

export function completionPercentage(completed: number, total: number): number {
  return total > 0 ? Math.round((completed / total) * 100) : 0;
}

export function isCourseExamUnlocked(completion: number): boolean {
  return completion >= COURSE_EXAM_REQUIRED_COMPLETION_PERCENTAGE;
}

export function hasReachedVideoCompletion(
  currentSeconds: number,
  durationSeconds: number,
): boolean {
  return durationSeconds > 0 && currentSeconds / durationSeconds >= 0.85;
}
