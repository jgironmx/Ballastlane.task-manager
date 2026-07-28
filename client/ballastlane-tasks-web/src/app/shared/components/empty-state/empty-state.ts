import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="empty-state">
      <p>{{ message() }}</p>
      <ng-content />
    </div>
  `,
  styles: `
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1rem;
      padding: 3rem 1rem;
      text-align: center;
      color: var(--color-text-muted, #555);
    }
  `,
})
export class EmptyState {
  readonly message = input.required<string>();
}
