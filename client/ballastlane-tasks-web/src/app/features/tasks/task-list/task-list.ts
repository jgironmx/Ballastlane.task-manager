import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/http/api-error.service';
import { NotificationService } from '../../../core/notifications/notification.service';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { LoadingIndicator } from '../../../shared/components/loading-indicator/loading-indicator';
import { formatDateOnly } from '../../../shared/utilities/date-only';
import { TaskItem, TaskItemStatus } from '../task.models';
import { TaskService } from '../task.service';
import { TaskStatusBadge } from '../task-status-badge/task-status-badge';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LoadingIndicator,
    EmptyState,
    ConfirmationDialog,
    TaskStatusBadge,
  ],
  templateUrl: './task-list.html',
  styleUrl: './task-list.scss',
})
export class TaskList {
  private readonly taskService = inject(TaskService);
  private readonly apiError = inject(ApiErrorService);
  private readonly notifications = inject(NotificationService);

  protected readonly tasks = signal<TaskItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly statusFilter = signal<TaskItemStatus | ''>('');
  protected readonly searchInput = signal('');

  protected readonly updatingStatusId = signal<string | null>(null);
  protected readonly deletingId = signal<string | null>(null);
  protected readonly pendingDeleteTask = signal<TaskItem | null>(null);

  protected readonly statuses: TaskItemStatus[] = ['Pending', 'InProgress', 'Completed'];
  protected readonly formatDateOnly = formatDateOnly;

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.taskService
      .getTasks({
        status: this.statusFilter() || undefined,
        search: this.searchInput().trim() || undefined,
      })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => this.tasks.set(result.items),
        error: (error: unknown) => this.errorMessage.set(this.apiError.normalize(error).message),
      });
  }

  onStatusFilterChange(): void {
    this.load();
  }

  onSearchSubmit(): void {
    this.load();
  }

  changeStatus(task: TaskItem, status: TaskItemStatus): void {
    if (status === task.status || this.updatingStatusId()) {
      return;
    }

    this.updatingStatusId.set(task.id);

    this.taskService
      .changeStatus(task.id, { status })
      .pipe(finalize(() => this.updatingStatusId.set(null)))
      .subscribe({
        next: (updated) => {
          this.tasks.update((list) => list.map((t) => (t.id === updated.id ? updated : t)));
        },
        error: (error: unknown) => {
          const normalized = this.apiError.normalize(error);
          if (normalized.kind === 'notFound') {
            this.removeStaleTask(task.id, "That task is no longer available — it's been removed from the list.");
            return;
          }
          this.notifications.error(normalized.message);
        },
      });
  }

  requestDelete(task: TaskItem): void {
    this.pendingDeleteTask.set(task);
  }

  cancelDelete(): void {
    this.pendingDeleteTask.set(null);
  }

  confirmDelete(): void {
    const task = this.pendingDeleteTask();
    if (!task || this.deletingId()) {
      return;
    }

    this.deletingId.set(task.id);
    this.pendingDeleteTask.set(null);

    this.taskService
      .deleteTask(task.id)
      .pipe(finalize(() => this.deletingId.set(null)))
      .subscribe({
        next: () => {
          this.tasks.update((list) => list.filter((t) => t.id !== task.id));
          this.notifications.success(`"${task.title}" was deleted.`);
        },
        error: (error: unknown) => {
          const normalized = this.apiError.normalize(error);
          if (normalized.kind === 'notFound') {
            this.removeStaleTask(task.id, "That task was already deleted.");
            return;
          }
          this.notifications.error(normalized.message);
        },
      });
  }

  private removeStaleTask(taskId: string, message: string): void {
    this.tasks.update((list) => list.filter((t) => t.id !== taskId));
    this.notifications.info(message);
  }
}
