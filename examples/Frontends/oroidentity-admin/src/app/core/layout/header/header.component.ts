import { Component, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { CompanySwitcherComponent } from '../../../features/company/company-switcher.component';
import { NotificationBellComponent } from '../../../features/notifications/notification-bell.component';
import { AuthService } from '../../auth/auth.service';
import { LayoutService } from '../../services/layout.service';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [NotificationBellComponent, CompanySwitcherComponent, TranslatePipe],
  template: `
    <header class="header">
      <div class="header-left">
        <button class="oro-btn oro-btn-ghost header-menu-btn"
                (click)="layout.toggleSidebar()"
                aria-label="Toggle sidebar">☰</button>
        <app-company-switcher />
      </div>
      <div class="header-right">
        <button class="oro-btn oro-btn-ghost" (click)="theme.toggle()" aria-label="Toggle theme">
          {{ theme.theme() === 'dark' ? '☀️' : '🌙' }}
        </button>
        <app-notification-bell />
        <div class="user-avatar" [title]="auth.displayName()">
          {{ initials() }}
        </div>
        <button class="oro-btn oro-btn-ghost" (click)="auth.logout()" [title]="'COMMON.CLOSE' | translate">⎋</button>
      </div>
    </header>
  `,
  styles: [`
    .header {
      position: fixed; top: 0; right: 0; left: var(--oro-sidebar-width);
      height: var(--oro-header-height); z-index: 50;
      background: var(--oro-surface); border-bottom: 1px solid var(--oro-border);
      display: flex; align-items: center; justify-content: space-between;
      padding: 0 var(--oro-space-4); gap: var(--oro-space-4);
      transition: left var(--oro-transition-slow);
    }
    .header-left, .header-right { display: flex; align-items: center; gap: var(--oro-space-2); }
    .header-menu-btn { font-size: 1.2rem; }
    .user-avatar {
      width: 32px; height: 32px; border-radius: var(--oro-radius-full);
      background: var(--oro-primary); color: white;
      display: flex; align-items: center; justify-content: center;
      font-size: var(--oro-text-xs); font-weight: 700; cursor: pointer;
    }
    :host-context([data-sidebar-collapsed]) .header { left: var(--oro-sidebar-collapsed); }
  `]
})
export class HeaderComponent {
  readonly layout = inject(LayoutService);
  readonly auth = inject(AuthService);
  readonly theme = inject(ThemeService);

  initials(): string {
    const name = this.auth.displayName();
    return name.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase() || 'U';
  }
}
