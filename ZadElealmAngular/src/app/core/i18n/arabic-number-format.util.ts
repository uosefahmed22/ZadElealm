const arabicNumberFormatter = new Intl.NumberFormat('ar-EG', {
  numberingSystem: 'arab',
  maximumFractionDigits: 2,
});

const arabicDateFormatter = new Intl.DateTimeFormat('ar-EG', {
  numberingSystem: 'arab',
  day: 'numeric',
  month: 'long',
  year: 'numeric',
});

const arabicDateTimeFormatter = new Intl.DateTimeFormat('ar-EG', {
  numberingSystem: 'arab',
  day: 'numeric',
  month: 'short',
  year: 'numeric',
  hour: 'numeric',
  minute: '2-digit',
});

const arabicDigits = '٠١٢٣٤٥٦٧٨٩';

export function localizeArabicDigits(value: string | number): string {
  return String(value).replace(/\d/g, (digit) => arabicDigits[Number(digit)]);
}

export function normalizeArabicDigits(value: string): string {
  return value.replace(/[٠-٩]/g, (digit) => String(arabicDigits.indexOf(digit)));
}

export function formatArabicNumber(value: number): string {
  return arabicNumberFormatter.format(value);
}

export function formatArabicDate(value: Date | string): string {
  const date = value instanceof Date ? value : new Date(value);
  return Number.isNaN(date.getTime()) ? '' : arabicDateFormatter.format(date);
}

export function formatArabicDateTime(value: Date | string): string {
  const date = value instanceof Date ? value : new Date(value);
  return Number.isNaN(date.getTime()) ? '' : arabicDateTimeFormatter.format(date);
}

export function formatArabicDuration(totalSeconds: number): string {
  const safeSeconds = Math.max(0, Math.floor(totalSeconds));
  const totalMinutes = Math.floor(safeSeconds / 60);
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  if (hours === 0) return `${formatArabicNumber(minutes)} دقيقة`;
  if (minutes === 0) return `${formatArabicNumber(hours)} ساعة`;
  return `${formatArabicNumber(hours)} ساعة و${formatArabicNumber(minutes)} دقيقة`;
}
