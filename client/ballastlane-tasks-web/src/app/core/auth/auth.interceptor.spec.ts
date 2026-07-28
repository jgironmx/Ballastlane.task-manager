import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { API_BASE_URL } from '../config/api-config';
import { authInterceptor } from './auth.interceptor';
import { AuthStore } from './auth.store';

const BASE_URL = 'http://localhost:5276';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;
  let authStore: AuthStore;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        { provide: API_BASE_URL, useValue: BASE_URL },
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    authStore = TestBed.inject(AuthStore);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should attach the bearer token to requests targeting the API when a valid token is present', () => {
    vi.spyOn(authStore, 'getValidAccessToken').mockReturnValue('token-abc');

    httpClient.get(`${BASE_URL}/api/tasks`).subscribe();

    const request = httpTesting.expectOne(`${BASE_URL}/api/tasks`);
    expect(request.request.headers.get('Authorization')).toBe('Bearer token-abc');
  });

  it('should not attach an Authorization header when no valid token is present', () => {
    vi.spyOn(authStore, 'getValidAccessToken').mockReturnValue(null);

    httpClient.get(`${BASE_URL}/api/tasks`).subscribe();

    const request = httpTesting.expectOne(`${BASE_URL}/api/tasks`);
    expect(request.request.headers.has('Authorization')).toBe(false);
  });

  it('should not attach the token to requests outside the API origin', () => {
    vi.spyOn(authStore, 'getValidAccessToken').mockReturnValue('token-abc');

    httpClient.get('https://external.example.com/data').subscribe();

    const request = httpTesting.expectOne('https://external.example.com/data');
    expect(request.request.headers.has('Authorization')).toBe(false);
  });
});

describe('authInterceptor with a relative (production-style) API_BASE_URL', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;
  let authStore: AuthStore;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        { provide: API_BASE_URL, useValue: '' },
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    authStore = TestBed.inject(AuthStore);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should attach the bearer token to same-origin relative requests', () => {
    vi.spyOn(authStore, 'getValidAccessToken').mockReturnValue('token-abc');

    httpClient.get('/api/tasks').subscribe();

    const request = httpTesting.expectOne('/api/tasks');
    expect(request.request.headers.get('Authorization')).toBe('Bearer token-abc');
  });

  it('should not attach the token to absolute external requests, even though API_BASE_URL is empty', () => {
    vi.spyOn(authStore, 'getValidAccessToken').mockReturnValue('token-abc');

    httpClient.get('https://external.example.com/data').subscribe();

    const request = httpTesting.expectOne('https://external.example.com/data');
    expect(request.request.headers.has('Authorization')).toBe(false);
  });
});
