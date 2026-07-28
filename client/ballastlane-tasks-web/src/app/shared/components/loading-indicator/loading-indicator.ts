import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-indicator',
  standalone: true,
  template: `
    <div class="loading" role="status">
      <span class="spinner" aria-hidden="true"></span>
      <span>{{ label() }}</span>
    </div>
  `,
  styles: `
    .loading {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 1.5rem;
      color: var(--color-text-muted, #555);
    }

    .spinner {
      width: 1.25rem;
      height: 1.25rem;
      border: 2px solid currentColor;
      border-top-color: transparent;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to {
        transform: rotate(360deg);
      }
    }

    @media (prefers-reduced-motion: reduce) {
      .spinner {
        animation: none;
      }
    }
  `,
})
export class LoadingIndicator {
  readonly label = input('Loading…');
}
