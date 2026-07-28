/** Matches Ballastlane.Tasks.Api.Contracts.Auth.RegisterRequest exactly. */
export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

/** Matches Ballastlane.Tasks.Api.Contracts.Auth.LoginRequest exactly. */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Matches Ballastlane.Tasks.Application.Contracts.UserDto exactly (register response and
 * GET /api/auth/me response). */
export interface AuthenticatedUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

/** Matches Ballastlane.Tasks.Api.Contracts.Auth.LoginResponse exactly. */
export interface LoginResponse {
  user: AuthenticatedUser;
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
}
