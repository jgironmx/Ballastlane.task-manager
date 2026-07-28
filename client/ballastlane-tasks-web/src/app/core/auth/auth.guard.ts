import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthStore } from './auth.store';

/** Protects authenticated-only routes. Waits for auth initialization to complete (never decides
 * from the mere presence of a token string) before allowing or redirecting. */
export const authGuard: CanActivateFn = (_route, state) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  return authStore.initialize().pipe(
    map(() =>
      authStore.isAuthenticated()
        ? true
        : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } }),
    ),
  );
};
