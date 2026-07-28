import { Injectable, signal } from '@angular/core';
import { AppNotification, NotificationKind } from './notification.model';

const AUTO_DISMISS_MS = 5000;

/** Minimal signal-based notification queue — no external toast library. */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly _notifications = signal<AppNotification[]>([]);
  private nextId = 1;

  readonly notifications = this._notifications.asReadonly();

  success(message: string): void {
    this.push('success', message, true);
  }

  error(message: string): void {
    this.push('error', message, false);
  }

  info(message: string): void {
    this.push('info', message, true);
  }

  dismiss(id: number): void {
    this._notifications.update((list) => list.filter((n) => n.id !== id));
  }

  private push(kind: NotificationKind, message: string, autoDismiss: boolean): void {
    const notification: AppNotification = { id: this.nextId++, kind, message };
    this._notifications.update((list) => [...list, notification]);

    if (autoDismiss) {
      setTimeout(() => this.dismiss(notification.id), AUTO_DISMISS_MS);
    }
  }
}
