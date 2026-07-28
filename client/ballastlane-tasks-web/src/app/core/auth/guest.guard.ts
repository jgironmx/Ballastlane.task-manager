import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthStore } from './auth.store';

/** Protects anonymous-only routes (login/register) — sends already-authenticated users to /tasks. */
export const guestGuard: CanActivateFn = () => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  return authStore
    .initialize()
    .pipe(map(() => (authStore.isAuthenticated() ? router.createUrlTree(['/tasks']) : true)));
};
