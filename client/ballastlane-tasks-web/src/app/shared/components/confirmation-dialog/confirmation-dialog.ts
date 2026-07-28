import { Component, ElementRef, afterRenderEffect, input, output, viewChild } from '@angular/core';

/**
 * A small accessible confirmation dialog — used instead of the browser's native `confirm()` so
 * focus can be managed and the message can include the task title. Moves focus to the cancel
 * button when opened (the safer default action) and closes on Escape or backdrop click.
 */
@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  template: `
    @if (open()) {
      <div class="backdrop" (click)="cancel.emit()">
        <div
          class="dialog"
          role="alertdialog"
          aria-modal="true"
          [attr.aria-labelledby]="titleId"
          [attr.aria-describedby]="messageId"
          (click)="$event.stopPropagation()"
          (keydown.escape)="cancel.emit()"
        >
          <h2 [id]="titleId">{{ title() }}</h2>
          <p [id]="messageId">{{ message() }}</p>
          <div class="actions">
            <button type="button" #cancelButton (click)="cancel.emit()">{{ cancelLabel() }}</button>
            <button type="button" class="danger" (click)="confirm.emit()">{{ confirmLabel() }}</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: `
    .backdrop {
      position: fixed;
      inset: 0;
      background: rgb(0 0 0 / 45%);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 1rem;
      z-index: 100;
    }

    .dialog {
      background: var(--color-surface, #fff);
      color: var(--color-text, #1a1a1a);
      border-radius: 0.5rem;
      padding: 1.5rem;
      max-width: 26rem;
      width: 100%;
      box-shadow: 0 10px 30px rgb(0 0 0 / 25%);
    }

    h2 {
      margin: 0 0 0.5rem;
      font-size: 1.125rem;
    }

    .actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      margin-top: 1.5rem;
    }

    button {
      padding: 0.5rem 1rem;
      border-radius: 0.375rem;
      border: 1px solid var(--color-border, #ccc);
      background: var(--color-surface, #fff);
      cursor: pointer;
    }

    button:focus-visible {
      outline: 2px solid var(--color-focus, #1a56db);
      outline-offset: 2px;
    }

    button.danger {
      background: var(--color-error, #b3261e);
      border-color: var(--color-error, #b3261e);
      color: #fff;
    }
  `,
})
export class ConfirmationDialog {
  readonly open = input.required<boolean>();
  readonly title = input('Confirm action');
  readonly message = input.required<string>();
  readonly confirmLabel = input('Confirm');
  readonly cancelLabel = input('Cancel');

  readonly confirm = output<void>();
  readonly cancel = output<void>();

  private readonly cancelButton = viewChild<ElementRef<HTMLButtonElement>>('cancelButton');

  protected readonly titleId = `confirmation-dialog-title-${crypto.randomUUID()}`;
  protected readonly messageId = `confirmation-dialog-message-${crypto.randomUUID()}`;

  constructor() {
    afterRenderEffect(() => {
      if (this.open()) {
        this.cancelButton()?.nativeElement.focus();
      }
    });
  }
}
