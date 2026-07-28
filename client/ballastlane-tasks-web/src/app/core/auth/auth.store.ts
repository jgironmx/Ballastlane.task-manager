import { Injectable, computed, signal } from '@angular/core';
import { Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { AuthService } from './auth.service';
import { AuthenticatedUser, LoginRequest, RegisterRequest } from './auth.models';
import { TokenStorageService } from './token-storage.service';

/**
 * Signal-based authentication state, shared across the app. `initialize()` is idempotent — it
 * caches its in-flight observable so route guards and the app shell can all call it on startup
 * without triggering duplicate `/api/auth/me` requests.
 */
@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly _user = signal<AuthenticatedUser | null>(null);
  private readonly _initialized = signal(false);
  private readonly _loading = signal(false);
  private initialization$: Observable<void> | null = null;

  readonly user = this._user.asReadonly();
  readonly isInitialized = this._initialized.asReadonly();
  readonly isLoading = this._loading.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);

  constructor(
    private readonly authService: AuthService,
    private readonly tokenStorage: TokenStorageService,
  ) {}

  /** Restores session from storage on app startup. Safe to call multiple times. */
  initialize(): Observable<void> {
    if (this.initialization$) {
      return this.initialization$;
    }

    const session = this.tokenStorage.getSession();
    if (!session || this.tokenStorage.isExpired(session.expiresAtUtc)) {
      this.tokenStorage.clear();
      this._user.set(null);
      this._initialized.set(true);
      this.initialization$ = of(void 0);
      return this.initialization$;
    }

    this.initialization$ = this.authService.getCurrentUser().pipe(
      tap((user) => this._user.set(user)),
      catchError(() => {
        // Token was rejected (expired server-side, revoked, etc.) — fall back to anonymous.
        this.tokenStorage.clear();
        this._user.set(null);
        return of(null);
      }),
      tap(() => this._initialized.set(true)),
      map(() => void 0),
      shareReplay(1),
    );

    return this.initialization$;
  }

  register(request: RegisterRequest): Observable<AuthenticatedUser> {
    this._loading.set(true);
    return this.authService.register(request).pipe(finalize(() => this._loading.set(false)));
  }

  /** Does not automatically authenticate — the caller must still call login(). */
  login(request: LoginRequest): Observable<AuthenticatedUser> {
    this._loading.set(true);
    return this.authService.login(request).pipe(
      tap((response) => {
        this.tokenStorage.setSession({
          accessToken: response.accessToken,
          expiresAtUtc: response.expiresAtUtc,
        });
        this._user.set(response.user);
      }),
      map((response) => response.user),
      finalize(() => this._loading.set(false)),
    );
  }

  logout(): void {
    this.tokenStorage.clear();
    this._user.set(null);
  }

  /** Called by the HTTP interceptor when an authenticated request comes back 401. */
  clearSession(): void {
    this.tokenStorage.clear();
    this._user.set(null);
  }

  getValidAccessToken(): string | null {
    const session = this.tokenStorage.getSession();
    if (!session || this.tokenStorage.isExpired(session.expiresAtUtc)) {
      return null;
    }

    return session.accessToken;
  }
}
