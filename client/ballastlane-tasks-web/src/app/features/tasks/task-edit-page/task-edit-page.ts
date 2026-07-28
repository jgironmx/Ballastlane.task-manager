import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/http/api-error.service';
import { NotificationService } from '../../../core/notifications/notification.service';
import { LoadingIndicator } from '../../../shared/components/loading-indicator/loading-indicator';
import { TaskItem } from '../task.models';
import { TaskForm, TaskFormValue } from '../task-form/task-form';
import { TaskService } from '../task.service';

/** Loads the task, then — on successful save — navigates back to the list, same as the create
 * page, for a single consistent post-save interaction across both forms. */
@Component({
  selector: 'app-task-edit-page',
  standalone: true,
  imports: [RouterLink, LoadingIndicator, TaskForm],
  template: `
    <section class="page">
      <a routerLink="/tasks" class="back-link">&larr; Back to tasks</a>
      <h1>Edit task</h1>

      @if (loading()) {
        <app-loading-indicator label="Loading task…" />
      } @else if (loadError(); as message) {
        <div class="error-state" role="alert">
          <p>{{ message }}</p>
          <a routerLink="/tasks" class="button">Back to tasks</a>
        </div>
      } @else if (task(); as loadedTask) {
        @if (saveError(); as message) {
          <p class="server-error" role="alert">{{ message }}</p>
        }

        <app-task-form
          [initialValue]="loadedTask"
          submitLabel="Save changes"
          [submitting]="submitting()"
          [enforceFutureDueDate]="false"
          (save)="onSave($event)"
        />
      }
    </section>
  `,
  styles: `
    .page {
      max-width: 32rem;
      margin: 0 auto;
    }

    .back-link {
      display: inline-block;
      margin-bottom: 1rem;
      text-decoration: none;
      font-size: 0.9375rem;
    }

    .server-error {
      color: var(--color-error, #b3261e);
    }

    .error-state {
      padding: 1.5rem;
      border: 1px solid var(--color-error, #b3261e);
      border-radius: 0.5rem;
      color: var(--color-error, #b3261e);
      display: flex;
      flex-direction: column;
      gap: 1rem;
      align-items: flex-start;
    }
  `,
})
export class TaskEditPage {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly taskService = inject(TaskService);
  private readonly apiError = inject(ApiErrorService);
  private readonly notifications = inject(NotificationService);

  protected readonly task = signal<TaskItem | null>(null);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly submitting = signal(false);
  protected readonly saveError = signal<string | null>(null);

  private readonly taskId: string;

  constructor() {
    this.taskId = this.route.snapshot.paramMap.get('id') ?? '';

    if (!this.taskId) {
      this.loading.set(false);
      this.loadError.set('Invalid task link.');
      return;
    }

    this.taskService
      .getTaskById(this.taskId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (task) => this.task.set(task),
        error: (error: unknown) => this.loadError.set(this.apiError.normalize(error).message),
      });
  }

  onSave(value: TaskFormValue): void {
    this.submitting.set(true);
    this.saveError.set(null);

    this.taskService
      .updateTask(this.taskId, value)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          this.notifications.success('Task updated.');
          void this.router.navigate(['/tasks']);
        },
        error: (error: unknown) => this.saveError.set(this.apiError.normalize(error).message),
      });
  }
}
