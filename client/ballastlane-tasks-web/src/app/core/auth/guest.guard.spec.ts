import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { vi } from 'vitest';
import { guestGuard } from './guest.guard';
import { AuthStore } from './auth.store';

describe('guestGuard', () => {
  let authStore: { initialize: ReturnType<typeof vi.fn>; isAuthenticated: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(() => {
    authStore = {
      initialize: vi.fn().mockReturnValue(of(void 0)),
      isAuthenticated: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [{ provide: AuthStore, useValue: authStore }],
    });

    router = TestBed.inject(Router);
  });

  function runGuard() {
    return TestBed.runInInjectionContext(() =>
      guestGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    );
  }

  it('should allow an anonymous user through', async () => {
    authStore.isAuthenticated.mockReturnValue(false);

    const value = await firstValueFrom(runGuard() as ReturnType<typeof of>);

    expect(value).toBe(true);
  });

  it('should redirect an already-authenticated user to /tasks', async () => {
    authStore.isAuthenticated.mockReturnValue(true);

    const value = await firstValueFrom(runGuard() as ReturnType<typeof of>);

    expect(value).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(value as UrlTree)).toBe('/tasks');
  });
});
