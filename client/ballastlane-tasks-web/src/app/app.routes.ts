import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'tasks', pathMatch: 'full' },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: 'tasks',
    canActivate: [authGuard],
    loadComponent: () => import('./features/tasks/task-list/task-list').then((m) => m.TaskList),
  },
  {
    path: 'tasks/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/tasks/task-create-page/task-create-page').then((m) => m.TaskCreatePage),
  },
  {
    path: 'tasks/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/tasks/task-edit-page/task-edit-page').then((m) => m.TaskEditPage),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/profile/profile-page/profile-page').then((m) => m.ProfilePage),
  },
  {
    path: '**',
    loadComponent: () => import('./pages/not-found/not-found').then((m) => m.NotFound),
  },
];
