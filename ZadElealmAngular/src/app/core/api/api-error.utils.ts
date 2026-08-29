import { HttpErrorResponse } from '@angular/common/http';

import {
  ApiValidationErrorEnvelope,
  NormalizedApiError,
  ProblemDetailsEnvelope
} from './api-error.models';

export function normalizeApiError(error: unknown): NormalizedApiError {
  if (error instanceof HttpErrorResponse) {
    const status = error.status || 0;
    const payload = error.error as
      | ApiValidationErrorEnvelope
      | ProblemDetailsEnvelope
      | string
      | null
      | undefined;

    if (typeof payload === 'string' && payload.trim()) {
      return {
        status,
        message: payload,
        errors: []
      };
    }

    if (payload && typeof payload === 'object') {
      const validationErrors = Array.isArray((payload as ApiValidationErrorEnvelope).errors)
        ? ((payload as ApiValidationErrorEnvelope).errors ?? []).filter(Boolean)
        : [];

      const message =
        (payload as ApiValidationErrorEnvelope).message?.trim() ||
        (payload as ProblemDetailsEnvelope).detail?.trim() ||
        (payload as ProblemDetailsEnvelope).title?.trim() ||
        fallbackMessage(status);

      return {
        status,
        message,
        errors: validationErrors,
        traceId: (payload as ProblemDetailsEnvelope).traceId
      };
    }

    return {
      status,
      message: fallbackMessage(status),
      errors: []
    };
  }

  if (error instanceof Error) {
    return {
      status: 0,
      message: error.message || fallbackMessage(0),
      errors: []
    };
  }

  return {
    status: 0,
    message: fallbackMessage(0),
    errors: []
  };
}

function fallbackMessage(status: number): string {
  if (status === 0) {
    return 'تعذر الاتصال بالخادم. حاول مرة أخرى.';
  }

  if (status >= 500) {
    return 'حدث خطأ غير متوقع. حاول مرة أخرى لاحقًا.';
  }

  if (status === 429) {
    return 'تم تجاوز عدد المحاولات المسموح بها مؤقتًا.';
  }

  return 'تعذر إتمام الطلب.';
}
