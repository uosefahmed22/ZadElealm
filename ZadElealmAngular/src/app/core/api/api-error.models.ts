export interface ApiResponseEnvelope {
  statusCode: number;
  message?: string | null;
}

export interface ApiDataResponseEnvelope<T> extends ApiResponseEnvelope {
  data: T;
}

export interface ApiValidationErrorEnvelope extends ApiResponseEnvelope {
  errors?: string[];
}

export interface ProblemDetailsEnvelope {
  status?: number;
  title?: string;
  detail?: string;
  traceId?: string;
}

export interface NormalizedApiError {
  status: number;
  message: string;
  errors: string[];
  traceId?: string;
}
