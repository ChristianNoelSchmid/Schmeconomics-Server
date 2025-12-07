import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  template: `
    <header>
      <nav class="flex justify-start">
        <a routerLink="" class="p-3">Home</a>
        <a routerLink="login" class="p-3">Login</a>
        <a routerLink="accounts" class="p-3">Account</a>
      </nav>
    </header>
    <main class="m-auto w-xl">
      <router-outlet />
    </main>
  `
})
export class App {
  protected readonly title = signal('Schmeconomics');
}
