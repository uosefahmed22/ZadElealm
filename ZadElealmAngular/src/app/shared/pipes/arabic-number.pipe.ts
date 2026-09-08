import { Pipe, PipeTransform } from '@angular/core';
import { formatArabicNumber } from '../../core/i18n/arabic-number-format.util';

@Pipe({
  name: 'arabicNumber',
  standalone: true,
})
export class ArabicNumberPipe implements PipeTransform {
  transform(value: number): string {
    return formatArabicNumber(value);
  }
}
