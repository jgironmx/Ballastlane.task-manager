export type NotificationKind = 'success' | 'error' | 'info';

export interface AppNotification {
  id: number;
  kind: NotificationKind;
  message: string;
}
