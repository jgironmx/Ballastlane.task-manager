import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <header class="app-header">
      <a routerLink="/tasks" class="brand">Ballastlane Task Manager</a>

      <nav aria-label="Main navigation">
        <a routerLink="/tasks" routerLinkActive="active">Tasks</a>
        <a routerLink="/profile" routerLinkActive="active">Profile</a>
      </nav>

      <div class="account">
        @if (authStore.user(); as user) {
          <span class="user-email">{{ user.email }}</span>
        }
        <button type="button" (click)="logout()">Log out</button>
      </div>
    </header>
  `,
  styles: `
    .app-header {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 1rem;
      padding: 0.75rem 1.5rem;
      border-bottom: 1px solid var(--color-border, #e0e0e0);
    }

    .brand {
      font-weight: 700;
      text-decoration: none;
      color: inherit;
      margin-right: auto;
    }

    nav {
      display: flex;
      gap: 1rem;
    }

    nav a {
      text-decoration: none;
      color: inherit;
      padding: 0.25rem 0;
      border-bottom: 2px solid transparent;
    }

    nav a.active {
      border-bottom-color: currentColor;
      font-weight: 600;
    }

    nav a:focus-visible,
    button:focus-visible {
      outline: 2px solid var(--color-focus, #1a56db);
      outline-offset: 2px;
    }

    .account {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .user-email {
      color: var(--color-text-muted, #555);
      font-size: 0.875rem;
    }

    button {
      padding: 0.4rem 0.9rem;
      border-radius: 0.375rem;
      border: 1px solid var(--color-border, #ccc);
      background: var(--color-surface, #fff);
      cursor: pointer;
    }

    @media (max-width: 40rem) {
      .app-header {
        justify-content: center;
        text-align: center;
      }

      .brand {
        margin-right: 0;
        width: 100%;
        text-align: center;
      }
    }
  `,
})
export class Header {
  protected readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  logout(): void {
    this.authStore.logout();
    void this.router.navigate(['/login']);
  }
}
