import { describe, expect, it } from 'vitest';

import {
  formatLatinDate,
  formatLatinDuration,
  formatLatinNumber,
} from './latin-number-format.util';

describe('Latin numeral formatting', () => {
  it('formats Arabic UI numbers with Latin digits', () => {
    expect(formatLatinNumber(8900)).toBe('8,900');
    expect(formatLatinNumber(47)).toBe('47');
  });

  it('formats durations without Arabic-Indic digits', () => {
    expect(formatLatinDuration(3900)).toBe('1 ساعة و5 دقيقة');
  });

  it('formats dates with Latin digits', () => {
    expect(formatLatinDate(new Date('2026-08-30T00:00:00Z'))).toMatch(/30.*2026/);
  });
});
