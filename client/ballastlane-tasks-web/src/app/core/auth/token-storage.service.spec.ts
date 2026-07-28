import { TestBed } from '@angular/core/testing';
import { TokenStorageService } from './token-storage.service';

describe('TokenStorageService', () => {
  let service: TokenStorageService;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(TokenStorageService);
  });

  it('should return null when nothing is stored', () => {
    expect(service.getSession()).toBeNull();
  });

  it('should store and retrieve a session', () => {
    service.setSession({ accessToken: 'abc123', expiresAtUtc: '2099-01-01T00:00:00Z' });

    expect(service.getSession()).toEqual({
      accessToken: 'abc123',
      expiresAtUtc: '2099-01-01T00:00:00Z',
    });
  });

  it('should clear the stored session', () => {
    service.setSession({ accessToken: 'abc123', expiresAtUtc: '2099-01-01T00:00:00Z' });
    service.clear();

    expect(service.getSession()).toBeNull();
  });

  it('should treat a past expiration as expired', () => {
    expect(service.isExpired('2000-01-01T00:00:00Z')).toBe(true);
  });

  it('should treat a future expiration as not expired', () => {
    expect(service.isExpired('2099-01-01T00:00:00Z')).toBe(false);
  });

  it('should treat an unparseable expiration as expired', () => {
    expect(service.isExpired('not-a-date')).toBe(true);
  });
});
