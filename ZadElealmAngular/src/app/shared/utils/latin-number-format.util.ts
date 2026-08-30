const numberFormatter = new Intl.NumberFormat('ar-EG', {
  numberingSystem: 'latn',
  maximumFractionDigits: 2,
});

const dateFormatter = new Intl.DateTimeFormat('ar-EG', {
  numberingSystem: 'latn',
  day: 'numeric',
  month: 'long',
  year: 'numeric',
});

export function formatLatinNumber(value: number): string {
  return numberFormatter.format(value);
}

export function formatLatinDate(value: Date | string): string {
  const date = value instanceof Date ? value : new Date(value);
  return Number.isNaN(date.getTime()) ? '' : dateFormatter.format(date);
}

export function formatLatinDuration(totalSeconds: number): string {
  const safeSeconds = Math.max(0, Math.floor(totalSeconds));
  const totalMinutes = Math.floor(safeSeconds / 60);
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  if (hours === 0) {
    return `${formatLatinNumber(minutes)} دقيقة`;
  }

  if (minutes === 0) {
    return `${formatLatinNumber(hours)} ساعة`;
  }

  return `${formatLatinNumber(hours)} ساعة و${formatLatinNumber(minutes)} دقيقة`;
}
