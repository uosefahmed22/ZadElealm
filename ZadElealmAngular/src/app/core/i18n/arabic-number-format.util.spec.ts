import {
  formatArabicDate,
  formatArabicDateTime,
  formatArabicDuration,
  formatArabicNumber,
  localizeArabicDigits,
  normalizeArabicDigits,
} from './arabic-number-format.util';

describe('Arabic numeral formatting', () => {
  it('formats numbers with Arabic digits', () => {
    expect(formatArabicNumber(8900)).toBe('٨٬٩٠٠');
    expect(formatArabicNumber(47.5)).toBe('٤٧٫٥');
  });

  it('localizes display text and normalizes numeric input', () => {
    expect(localizeArabicDigits('12:05')).toBe('١٢:٠٥');
    expect(normalizeArabicDigits('١٢٣٤')).toBe('1234');
  });

  it('formats durations and dates with Arabic digits', () => {
    expect(formatArabicDuration(3900)).toBe('١ ساعة و٥ دقيقة');
    expect(formatArabicDate(new Date('2026-08-30T00:00:00Z'))).toMatch(/٣٠.*٢٠٢٦/);
    expect(formatArabicDateTime(new Date('2026-08-30T12:05:00Z'))).toMatch(/٣٠.*٢٠٢٦/);
  });
});
