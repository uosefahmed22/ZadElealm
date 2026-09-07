import {
  completionPercentage,
  formatDuration,
  hasReachedVideoCompletion,
  isCourseExamUnlocked,
  timeSpanToSeconds,
} from './learning.models';

describe('learning progress helpers', () => {
  it('calculates completed-video percentages', () => {
    expect(completionPercentage(4, 5)).toBe(80);
    expect(completionPercentage(3, 5)).toBe(60);
  });

  it('opens a course exam only after all videos are complete', () => {
    expect(isCourseExamUnlocked(99)).toBe(false);
    expect(isCourseExamUnlocked(100)).toBe(true);
  });

  it('marks a video complete only at eighty-five percent', () => {
    expect(hasReachedVideoCompletion(84, 100)).toBe(false);
    expect(hasReachedVideoCompletion(85, 100)).toBe(true);
  });

  it('parses and formats backend TimeSpan values', () => {
    expect(timeSpanToSeconds('00:02:05')).toBe(125);
    expect(formatDuration('00:02:05')).toBe('2:05');
  });
});
