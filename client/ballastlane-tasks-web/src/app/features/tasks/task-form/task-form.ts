import { Component, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { FieldError } from '../../../shared/components/field-error/field-error';
import { isDateOnlyBefore, todayAsDateOnly } from '../../../shared/utilities/date-only';
import { TaskItem } from '../task.models';

export interface TaskFormValue {
  title: string;
  description: string | null;
  dueDate: string | null;
}

/**
 * Shared reactive form for both create and edit. `enforceFutureDueDate` defaults to `true`
 * (create) — the backend only rejects a past due date at creation, not on update (an existing
 * task's due date can legitimately drift into the past while it sits open — see
 * docs/decisions/ADR-006-taskitem-domain-model.md), so the edit page passes `false`.
 */
@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [ReactiveFormsModule, FieldError],
  templateUrl: './task-form.html',
  styleUrl: './task-form.scss',
})
export class TaskForm {
  private readonly formBuilder = inject(FormBuilder);

  readonly initialValue = input<TaskItem | null>(null);
  readonly submitting = input(false);
  readonly submitLabel = input('Save');
  readonly enforceFutureDueDate = input(true);

  readonly save = output<TaskFormValue>();

  protected readonly form = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    dueDate: ['', [this.dueDateValidator()]],
  });

  constructor() {
    effect(() => {
      const value = this.initialValue();
      if (value) {
        this.form.patchValue({
          title: value.title,
          description: value.description ?? '',
          dueDate: value.dueDate ?? '',
        });
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { title, description, dueDate } = this.form.getRawValue();
    this.save.emit({
      title: title.trim(),
      description: description.trim() || null,
      dueDate: dueDate || null,
    });
  }

  private dueDateValidator(): ValidatorFn {
    return (control) => {
      if (!this.enforceFutureDueDate() || !control.value) {
        return null;
      }

      return isDateOnlyBefore(control.value, todayAsDateOnly()) ? { pastDate: true } : null;
    };
  }
}
