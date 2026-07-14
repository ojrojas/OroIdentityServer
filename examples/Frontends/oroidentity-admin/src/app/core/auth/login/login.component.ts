import { Component, inject } from '@angular/core';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  template: `
    <div class="login-page">
      <div class="login-card oro-card">
        <div class="login-brand">
          <span class="login-logo">SERVER</span>
          <h1>OroIdentityServer Admin</h1>
          <p class="text-muted text-sm">Server Identity</p>
        </div>
        <button class="oro-btn oro-btn-primary login-btn" (click)="login()">
          Login
        </button>
      </div>
    </div>
  `,
  styles: [`
    .login-page {
      height: 100vh; display: flex; align-items: center;
      justify-content: center; background: var(--oro-bg);
    }
    .login-card {
      width: 380px; text-align: center;
      display: flex; flex-direction: column; gap: var(--oro-space-6);
    }
    .login-brand { display: flex; flex-direction: column; align-items: center; gap: var(--oro-space-2); }
    .login-logo { font-size: 3rem; }
    .login-brand h1 { font-size: var(--oro-text-2xl); font-weight: 700; color: var(--oro-primary); }
    .login-btn { width: 100%; justify-content: center; padding: var(--oro-space-3); }
  `]
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  login(): void { this.auth.login(); }
}
