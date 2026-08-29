import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

const arabicPattern = /^[\u0600-\u06FF\s]+$/;

export function arabicNameValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = `${control.value ?? ''}`.trim();
    if (!value) {
      return null;
    }

    return arabicPattern.test(value) ? null : { arabicName: true };
  };
}
