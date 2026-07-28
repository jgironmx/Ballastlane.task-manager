import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthStore } from './core/auth/auth.store';
import { Header } from './layout/header/header';
import { NotificationHost } from './shared/components/notification-host/notification-host';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, NotificationHost],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly authStore = inject(AuthStore);
}
