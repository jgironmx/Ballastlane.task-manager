import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { AuthenticatedUser, LoginRequest, LoginResponse, RegisterRequest } from './auth.models';

/** Thin HTTP wrapper over /api/auth/*. Holds no state — AuthStore owns state. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    private readonly http: HttpClient,
    @Inject(API_BASE_URL) private readonly baseUrl: string,
  ) {}

  register(request: RegisterRequest): Observable<AuthenticatedUser> {
    return this.http.post<AuthenticatedUser>(`${this.baseUrl}/api/auth/register`, request);
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/api/auth/login`, request);
  }

  getCurrentUser(): Observable<AuthenticatedUser> {
    return this.http.get<AuthenticatedUser>(`${this.baseUrl}/api/auth/me`);
  }
}
