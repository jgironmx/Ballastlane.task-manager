import { Component, input } from '@angular/core';

/** Renders a single field-level validation message, linked to its input via `id` +
 * the input's `aria-describedby`. Renders nothing when `message` is null. */
@Component({
  selector: 'app-field-error',
  standalone: true,
  template: `
    @if (message(); as text) {
      <p [id]="id()" class="field-error" role="alert">{{ text }}</p>
    }
  `,
  styles: `
    .field-error {
      margin: 0.25rem 0 0;
      color: var(--color-error, #b3261e);
      font-size: 0.875rem;
    }
  `,
})
export class FieldError {
  readonly id = input.required<string>();
  readonly message = input<string | null>(null);
}
