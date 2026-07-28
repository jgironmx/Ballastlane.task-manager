import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { vi } from 'vitest';
import { authGuard } from './auth.guard';
import { AuthStore } from './auth.store';

describe('authGuard', () => {
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

  function runGuard(url: string) {
    return TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    );
  }

  it('should allow an authenticated user through', async () => {
    authStore.isAuthenticated.mockReturnValue(true);

    const value = await firstValueFrom(runGuard('/tasks') as ReturnType<typeof of>);

    expect(value).toBe(true);
  });

  it('should redirect an anonymous user to /login with a return URL', async () => {
    authStore.isAuthenticated.mockReturnValue(false);

    const value = await firstValueFrom(runGuard('/tasks/new') as ReturnType<typeof of>);

    expect(value).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(value as UrlTree)).toBe('/login?returnUrl=%2Ftasks%2Fnew');
  });
});
