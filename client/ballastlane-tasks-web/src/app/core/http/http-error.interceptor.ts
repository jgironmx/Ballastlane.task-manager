import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from '../auth/auth.store';
import { NotificationService } from '../notifications/notification.service';

/** Endpoints whose own 401 response means "wrong credentials," not "your session expired" —
 * excluded from the global session-expiry redirect. */
const AUTH_ENDPOINTS = ['/api/auth/login', '/api/auth/register'];

/** Reacts to 401 on any *other* authenticated request: clears the session, notifies the user,
 * and redirects to /login with a return URL. */
export const httpErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const router = inject(Router);
  const authStore = inject(AuthStore);
  const notifications = inject(NotificationService);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !AUTH_ENDPOINTS.some((endpoint) => request.url.includes(endpoint))
      ) {
        const wasAuthenticated = authStore.isAuthenticated();
        authStore.clearSession();

        if (wasAuthenticated) {
          notifications.info('Your session expired. Please sign in again.');
          void router.navigate(['/login'], {
            queryParams: { returnUrl: router.url },
          });
        }
      }

      return throwError(() => error);
    }),
  );
};
