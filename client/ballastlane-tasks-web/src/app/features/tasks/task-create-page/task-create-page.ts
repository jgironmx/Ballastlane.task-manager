import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/http/api-error.service';
import { NotificationService } from '../../../core/notifications/notification.service';
import { TaskForm, TaskFormValue } from '../task-form/task-form';
import { TaskService } from '../task.service';

@Component({
  selector: 'app-task-create-page',
  standalone: true,
  imports: [RouterLink, TaskForm],
  template: `
    <section class="page">
      <a routerLink="/tasks" class="back-link">&larr; Back to tasks</a>
      <h1>New task</h1>

      @if (errorMessage(); as message) {
        <p class="server-error" role="alert">{{ message }}</p>
      }

      <app-task-form
        submitLabel="Create task"
        [submitting]="submitting()"
        [enforceFutureDueDate]="true"
        (save)="onSave($event)"
      />
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
  `,
})
export class TaskCreatePage {
  private readonly taskService = inject(TaskService);
  private readonly apiError = inject(ApiErrorService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  onSave(value: TaskFormValue): void {
    this.submitting.set(true);
    this.errorMessage.set(null);

    this.taskService
      .createTask(value)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          this.notifications.success('Task created.');
          void this.router.navigate(['/tasks']);
        },
        error: (error: unknown) => this.errorMessage.set(this.apiError.normalize(error).message),
      });
  }
}
