import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section>
      <h1>Page not found</h1>
      <p>The page you're looking for doesn't exist.</p>
      <a routerLink="/">Go to Tasks</a>
    </section>
  `,
  styles: `
    section {
      text-align: center;
      padding: 3rem 1rem;
    }
  `,
})
export class NotFound {}
