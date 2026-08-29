import { FormControl, FormGroup } from '@angular/forms';

import { passwordMatchValidator } from './password-match.validator';

describe('passwordMatchValidator', () => {
  it('returns null when passwords match', () => {
    const form = new FormGroup(
      {
        newPassword: new FormControl('12345678'),
        confirmPassword: new FormControl('12345678')
      },
      { validators: [passwordMatchValidator('newPassword', 'confirmPassword')] }
    );

    expect(form.errors).toBeNull();
  });

  it('returns passwordMismatch when passwords differ', () => {
    const form = new FormGroup(
      {
        newPassword: new FormControl('12345678'),
        confirmPassword: new FormControl('87654321')
      },
      { validators: [passwordMatchValidator('newPassword', 'confirmPassword')] }
    );

    expect(form.errors).toEqual({ passwordMismatch: true });
  });
});
