import { HttpErrorResponse } from '@angular/common/http';

import { normalizeApiError } from './api-error.utils';

describe('normalizeApiError', () => {
  it('extracts validation errors from api payload', () => {
    const result = normalizeApiError(
      new HttpErrorResponse({
        status: 400,
        error: {
          statusCode: 400,
          message: 'فشل التحقق',
          errors: ['البريد الإلكتروني مطلوب', 'كلمة المرور مطلوبة']
        }
      })
    );

    expect(result.status).toBe(400);
    expect(result.message).toBe('فشل التحقق');
    expect(result.errors).toEqual(['البريد الإلكتروني مطلوب', 'كلمة المرور مطلوبة']);
  });

  it('falls back to problem details when api message is absent', () => {
    const result = normalizeApiError(
      new HttpErrorResponse({
        status: 500,
        error: {
          title: 'حدث خطأ غير متوقع.',
          detail: 'يرجى المحاولة لاحقاً.',
          traceId: 'trace-123'
        }
      })
    );

    expect(result.message).toBe('يرجى المحاولة لاحقاً.');
    expect(result.traceId).toBe('trace-123');
  });
});
