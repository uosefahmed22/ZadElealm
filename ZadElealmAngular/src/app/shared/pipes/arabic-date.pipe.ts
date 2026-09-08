import { Pipe, PipeTransform } from '@angular/core';

import { formatArabicDate, formatArabicDateTime } from '../../core/i18n/arabic-number-format.util';

@Pipe({
  name: 'arabicDate',
  standalone: true,
})
export class ArabicDatePipe implements PipeTransform {
  transform(value: string | Date | null | undefined, mode: 'date' | 'dateTime' = 'date'): string {
    if (value === null || value === undefined) return '';
    return mode === 'dateTime' ? formatArabicDateTime(value) : formatArabicDate(value);
  }
}
