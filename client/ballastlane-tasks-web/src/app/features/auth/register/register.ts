import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthStore } from '../../../core/auth/auth.store';
import { ApiErrorService } from '../../../core/http/api-error.service';
import { NotificationService } from '../../../core/notifications/notification.service';
import { FieldError } from '../../../shared/components/field-error/field-error';
import { passwordMatchValidator } from '../../../shared/validators/password-match.validator';

// Mirrors the backend's ASP.NET Core Identity policy (RequiredLength=8, RequireNonAlphanumeric=false,
// RequireDigit/RequireUppercase/RequireLowercase left at their Identity defaults of true — see
// InfrastructureServiceCollectionExtensions.cs). This is client-side *guidance* only; the backend
// remains authoritative and its own validation errors are always displayed alongside this.
const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, FieldError],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authStore = inject(AuthStore);
  private readonly apiError = inject(ApiErrorService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly errorDetails = signal<string[] | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group(
    {
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(PASSWORD_PATTERN)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordMatchValidator('password', 'confirmPassword') },
  );

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    this.errorDetails.set(null);

    const { firstName, lastName, email, password } = this.form.getRawValue();

    this.authStore
      .register({
        email: email.trim(),
        password,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
      })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          this.notifications.success('Account created. Please sign in.');
          void this.router.navigate(['/login']);
        },
        error: (error: unknown) => {
          const normalized = this.apiError.normalize(error);
          this.errorMessage.set(normalized.message);
          this.errorDetails.set(normalized.details ?? null);
        },
      });
  }
}
