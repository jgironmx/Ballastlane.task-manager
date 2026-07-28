import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_BASE_URL } from '../config/api-config';
import { AuthStore } from './auth.store';

/** Attaches `Authorization: Bearer <token>` only to requests targeting our own API origin, and
 * only when a valid (non-expired) token is present. Never touches unrelated/external requests.
 *
 * Origins are compared via the URL API rather than `startsWith`, because in production
 * `API_BASE_URL` is `''` (a relative `/api` path resolved against the current origin) — a plain
 * `startsWith('')` would be vacuously true for every URL, including third-party ones. */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const baseUrl = inject(API_BASE_URL);
  const authStore = inject(AuthStore);

  const requestOrigin = new URL(request.url, window.location.origin).origin;
  const apiOrigin = new URL(baseUrl || window.location.origin, window.location.origin).origin;

  if (requestOrigin !== apiOrigin) {
    return next(request);
  }

  const token = authStore.getValidAccessToken();
  if (!token) {
    return next(request);
  }

  return next(
    request.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    }),
  );
};
