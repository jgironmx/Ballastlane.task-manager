import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { AuthenticatedUser, LoginResponse } from './auth.models';
import { AuthService } from './auth.service';
import { AuthStore } from './auth.store';
import { TokenStorageService } from './token-storage.service';

const user: AuthenticatedUser = {
  id: 'user-1',
  email: 'jane@example.com',
  firstName: 'Jane',
  lastName: 'Doe',
};

const loginResponse: LoginResponse = {
  user,
  accessToken: 'token-abc',
  tokenType: 'Bearer',
  expiresAtUtc: '2099-01-01T00:00:00Z',
};

describe('AuthStore', () => {
  let store: AuthStore;
  let authService: { login: ReturnType<typeof vi.fn>; register: ReturnType<typeof vi.fn>; getCurrentUser: ReturnType<typeof vi.fn> };
  let tokenStorage: TokenStorageService;

  beforeEach(() => {
    sessionStorage.clear();
    authService = {
      login: vi.fn(),
      register: vi.fn(),
      getCurrentUser: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [{ provide: AuthService, useValue: authService }],
    });

    store = TestBed.inject(AuthStore);
    tokenStorage = TestBed.inject(TokenStorageService);
  });

  it('should store the token on successful login', async () => {
    authService.login.mockReturnValue(of(loginResponse));

    await firstValueFrom(store.login({ email: user.email, password: 'Password1!' }));

    expect(tokenStorage.getSession()?.accessToken).toBe('token-abc');
    expect(store.user()).toEqual(user);
    expect(store.isAuthenticated()).toBe(true);
  });

  it('should not store a token when login fails', async () => {
    authService.login.mockReturnValue(throwError(() => new Error('invalid credentials')));

    await expect(
      firstValueFrom(store.login({ email: user.email, password: 'wrong' })),
    ).rejects.toThrow();

    expect(tokenStorage.getSession()).toBeNull();
    expect(store.isAuthenticated()).toBe(false);
  });

  it('should clear the token and user on logout', async () => {
    authService.login.mockReturnValue(of(loginResponse));
    await firstValueFrom(store.login({ email: user.email, password: 'Password1!' }));

    store.logout();

    expect(tokenStorage.getSession()).toBeNull();
    expect(store.user()).toBeNull();
    expect(store.isAuthenticated()).toBe(false);
  });

  it('should produce an anonymous state when initializing with no stored token', async () => {
    await firstValueFrom(store.initialize());

    expect(store.isAuthenticated()).toBe(false);
    expect(store.isInitialized()).toBe(true);
    expect(authService.getCurrentUser).not.toHaveBeenCalled();
  });

  it('should load the current user when initializing with a valid stored token', async () => {
    tokenStorage.setSession({ accessToken: 'token-abc', expiresAtUtc: '2099-01-01T00:00:00Z' });
    authService.getCurrentUser.mockReturnValue(of(user));

    await firstValueFrom(store.initialize());

    expect(store.user()).toEqual(user);
    expect(store.isAuthenticated()).toBe(true);
  });

  it('should clear state when initializing with a token the server rejects', async () => {
    tokenStorage.setSession({ accessToken: 'stale-token', expiresAtUtc: '2099-01-01T00:00:00Z' });
    authService.getCurrentUser.mockReturnValue(throwError(() => new Error('401')));

    await firstValueFrom(store.initialize());

    expect(store.user()).toBeNull();
    expect(store.isAuthenticated()).toBe(false);
    expect(tokenStorage.getSession()).toBeNull();
  });

  it('should not issue duplicate /me requests when initialize is called more than once', async () => {
    tokenStorage.setSession({ accessToken: 'token-abc', expiresAtUtc: '2099-01-01T00:00:00Z' });
    authService.getCurrentUser.mockReturnValue(of(user));

    await firstValueFrom(store.initialize());
    await firstValueFrom(store.initialize());

    expect(authService.getCurrentUser).toHaveBeenCalledTimes(1);
  });
});
