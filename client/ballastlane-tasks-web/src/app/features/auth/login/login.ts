import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthStore } from '../../../core/auth/auth.store';
import { ApiErrorService } from '../../../core/http/api-error.service';
import { FieldError } from '../../../shared/components/field-error/field-error';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, FieldError],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authStore = inject(AuthStore);
  private readonly apiError = inject(ApiErrorService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly isDevelopment = !environment.production;
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { email, password } = this.form.getRawValue();

    this.authStore
      .login({ email: email.trim(), password })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/tasks';
          void this.router.navigateByUrl(returnUrl);
        },
        error: (error: unknown) => {
          // The backend never reveals whether the email exists — surface its generic message as-is.
          this.errorMessage.set(this.apiError.normalize(error).message);
        },
      });
  }
}
