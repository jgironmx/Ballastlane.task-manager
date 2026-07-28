import { Component, inject } from '@angular/core';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  template: `
    <section class="profile">
      <h1>Profile</h1>

      @if (authStore.user(); as user) {
        <dl>
          <dt>First name</dt>
          <dd>{{ user.firstName }}</dd>

          <dt>Last name</dt>
          <dd>{{ user.lastName }}</dd>

          <dt>Email</dt>
          <dd>{{ user.email }}</dd>

          <dt class="muted">User ID</dt>
          <dd class="muted">{{ user.id }}</dd>
        </dl>
      }
    </section>
  `,
  styles: `
    .profile {
      max-width: 32rem;
      margin: 0 auto;
    }

    dl {
      display: grid;
      grid-template-columns: auto 1fr;
      gap: 0.5rem 1rem;
    }

    dt {
      font-weight: 600;
    }

    dd {
      margin: 0;
    }

    .muted {
      color: var(--color-text-muted, #777);
      font-size: 0.8125rem;
    }
  `,
})
export class ProfilePage {
  protected readonly authStore = inject(AuthStore);
}
