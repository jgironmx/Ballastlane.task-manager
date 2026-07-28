import { Component, inject } from '@angular/core';
import { NotificationService } from '../../../core/notifications/notification.service';

/** A single ARIA live region rendering the current notification queue. Mounted once in the app shell. */
@Component({
  selector: 'app-notification-host',
  standalone: true,
  template: `
    <div class="notification-host" role="status" aria-live="polite">
      @for (notification of notifications.notifications(); track notification.id) {
        <div class="notification" [class]="notification.kind">
          <span>{{ notification.message }}</span>
          <button type="button" (click)="notifications.dismiss(notification.id)" aria-label="Dismiss notification">
            &times;
          </button>
        </div>
      }
    </div>
  `,
  styles: `
    .notification-host {
      position: fixed;
      top: 1rem;
      right: 1rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      z-index: 200;
      max-width: min(24rem, calc(100vw - 2rem));
    }

    .notification {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      border-radius: 0.375rem;
      box-shadow: 0 4px 12px rgb(0 0 0 / 20%);
      font-size: 0.9375rem;
      color: #fff;
    }

    .notification.success {
      background: #1e7e34;
    }

    .notification.error {
      background: #b3261e;
    }

    .notification.info {
      background: #1a56db;
    }

    .notification button {
      background: transparent;
      border: none;
      color: inherit;
      cursor: pointer;
      font-size: 1.125rem;
      line-height: 1;
      padding: 0;
    }
  `,
})
export class NotificationHost {
  protected readonly notifications = inject(NotificationService);
}
