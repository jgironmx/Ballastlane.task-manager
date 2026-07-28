import { Injectable } from '@angular/core';

const TOKEN_KEY = 'ballastlane.accessToken';
const EXPIRES_AT_KEY = 'ballastlane.expiresAtUtc';

export interface StoredSession {
  accessToken: string;
  expiresAtUtc: string;
}

/**
 * Stores the JWT in `sessionStorage` — see docs/decisions/ADR-011-spa-token-storage.md for the
 * rationale and its security trade-offs. Only the token and its expiration are stored; the
 * profile itself is always re-fetched from `/api/auth/me` rather than trusted from storage.
 */
@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getSession(): StoredSession | null {
    const accessToken = sessionStorage.getItem(TOKEN_KEY);
    const expiresAtUtc = sessionStorage.getItem(EXPIRES_AT_KEY);

    if (!accessToken || !expiresAtUtc) {
      return null;
    }

    return { accessToken, expiresAtUtc };
  }

  setSession(session: StoredSession): void {
    sessionStorage.setItem(TOKEN_KEY, session.accessToken);
    sessionStorage.setItem(EXPIRES_AT_KEY, session.expiresAtUtc);
  }

  clear(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(EXPIRES_AT_KEY);
  }

  isExpired(expiresAtUtc: string): boolean {
    const expiry = Date.parse(expiresAtUtc);
    return Number.isNaN(expiry) || expiry <= Date.now();
  }
}
