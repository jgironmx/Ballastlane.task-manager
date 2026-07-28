import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiError, ApiProblemDetails } from './api-error.model';

/** Turns any HttpClient failure into a normalized, UI-safe ApiError — never exposes raw
 * exception text or stack traces to the user. */
@Injectable({ providedIn: 'root' })
export class ApiErrorService {
  normalize(error: unknown): ApiError {
    if (!(error instanceof HttpErrorResponse)) {
      return { kind: 'unknown', status: 0, message: 'Something went wrong. Please try again.' };
    }

    if (error.status === 0) {
      return {
        kind: 'network',
        status: 0,
        message: 'Unable to reach the server. Check your connection and try again.',
      };
    }

    const problem = this.asProblemDetails(error.error);

    switch (error.status) {
      case 400:
        return {
          kind: 'validation',
          status: 400,
          message: problem?.title ?? 'The request was invalid.',
          details: problem?.errors,
        };
      case 401:
        return {
          kind: 'unauthorized',
          status: 401,
          message: problem?.title ?? 'Invalid email or password.',
        };
      case 404:
        return {
          kind: 'notFound',
          status: 404,
          message: problem?.title ?? 'The requested item could not be found.',
        };
      case 409:
        return {
          kind: 'conflict',
          status: 409,
          message: problem?.title ?? 'This conflicts with existing data.',
        };
      default:
        return {
          kind: error.status >= 500 ? 'server' : 'unknown',
          status: error.status,
          message: problem?.title ?? 'An unexpected error occurred. Please try again.',
        };
    }
  }

  private asProblemDetails(body: unknown): ApiProblemDetails | null {
    if (body !== null && typeof body === 'object' && ('title' in body || 'type' in body)) {
      return body as ApiProblemDetails;
    }

    return null;
  }
}
