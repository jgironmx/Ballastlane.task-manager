/**
 * Matches the actual JSON shape returned by the backend's Problem Details responses
 * (see ResultProblemExtensions.cs / GlobalExceptionHandler.cs), confirmed by inspecting real
 * responses from the running API. Note this is a flat string array, not the ASP.NET Core MVC
 * default `Record<string, string[]>` shape a naive assumption might expect.
 */
export interface ApiProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  traceId?: string;
  errors?: string[];
}

export type ApiErrorKind =
  | 'network'
  | 'validation'
  | 'unauthorized'
  | 'notFound'
  | 'conflict'
  | 'server'
  | 'unknown';

/** A normalized, UI-friendly representation of any API failure. */
export interface ApiError {
  kind: ApiErrorKind;
  status: number;
  message: string;
  details?: string[];
}
