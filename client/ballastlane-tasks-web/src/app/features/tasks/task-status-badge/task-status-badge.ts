import { Component, input } from '@angular/core';
import { TaskItemStatus } from '../task.models';

const STATUS_LABELS: Record<TaskItemStatus, string> = {
  Pending: 'Pending',
  InProgress: 'In progress',
  Completed: 'Completed',
};

/** Status is conveyed by text label, not color alone (color is a secondary cue only). */
@Component({
  selector: 'app-task-status-badge',
  standalone: true,
  template: `<span class="badge" [class]="status()">{{ label() }}</span>`,
  styles: `
    .badge {
      display: inline-block;
      padding: 0.2rem 0.6rem;
      border-radius: 999px;
      font-size: 0.8125rem;
      font-weight: 600;
      border: 1px solid transparent;
    }

    .Pending {
      background: color-mix(in srgb, #8a8a8a 15%, transparent);
      border-color: #8a8a8a;
      color: var(--color-text, #333);
    }

    .InProgress {
      background: color-mix(in srgb, #1a56db 15%, transparent);
      border-color: #1a56db;
      color: var(--color-text, #1a56db);
    }

    .Completed {
      background: color-mix(in srgb, #1e7e34 15%, transparent);
      border-color: #1e7e34;
      color: var(--color-text, #1e7e34);
    }
  `,
})
export class TaskStatusBadge {
  readonly status = input.required<TaskItemStatus>();
  protected readonly label = () => STATUS_LABELS[this.status()];
}
