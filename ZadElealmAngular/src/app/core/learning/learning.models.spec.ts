import {
  completionPercentage,
  formatDuration,
  hasReachedVideoCompletion,
  timeSpanToSeconds,
} from './learning.models';

describe('learning progress helpers', () => {
  it('opens a course quiz at eighty percent completed videos', () => {
    expect(completionPercentage(4, 5)).toBe(80);
    expect(completionPercentage(3, 5)).toBe(60);
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
