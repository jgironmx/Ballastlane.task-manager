import { InjectionToken } from '@angular/core';
import { environment } from '../../../environments/environment';

/** Single source of truth for the backend origin. Change per environment file, not per call site. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => environment.apiBaseUrl,
});
