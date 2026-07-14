import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { debounceTime, Subject } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { SettingsStore } from '../../../features/settings/settings.store';
import { LayoutService } from '../../services/layout.service';

interface NavItem {
  icon: string;
  labelKey: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, TranslatePipe],
  template: `
    <nav class="sidebar" [class.collapsed]="layout.sidebarCollapsed()">
      <div class="sidebar-brand">
        <span class="sidebar-logo">🏦</span>
        @if (!layout.sidebarCollapsed()) {
          <span class="sidebar-title">OroIdentityServer</span>
        }
      </div>
      <ul class="sidebar-nav">
        @for (item of navItems; track item.route) {
          <li>
            <a class="sidebar-link" [routerLink]="item.route" routerLinkActive="active"
               [title]="layout.sidebarCollapsed() ? (item.labelKey | translate) : ''">
              <span class="sidebar-icon">{{ item.icon }}</span>
              @if (!layout.sidebarCollapsed()) {
                <span class="sidebar-label">{{ item.labelKey | translate }}</span>
              }
            </a>
          </li>
        }
      </ul>
      <button class="sidebar-toggle oro-btn oro-btn-ghost" (click)="onToggle()">
        {{ layout.sidebarCollapsed() ? '→' : '←' }}
      </button>
    </nav>
  `,
  styles: [`
    .sidebar {
      display: flex; flex-direction: column;
      width: var(--oro-sidebar-width); height: 100vh;
      background: var(--oro-surface); border-right: 1px solid var(--oro-border);
      transition: width var(--oro-transition-slow);
      position: fixed; left: 0; top: 0; z-index: 100;
      overflow: hidden;
    }
    .sidebar.collapsed { width: var(--oro-sidebar-collapsed); }
    .sidebar-brand {
      display: flex; align-items: center; gap: var(--oro-space-3);
      padding: var(--oro-space-4); height: var(--oro-header-height);
      border-bottom: 1px solid var(--oro-border);
    }
    .sidebar-logo { font-size: 1.5rem; flex-shrink: 0; }
    .sidebar-title { font-weight: 700; color: var(--oro-primary); white-space: nowrap; }
    .sidebar-nav { flex: 1; list-style: none; padding: var(--oro-space-2) 0; overflow-y: auto; }
    .sidebar-link {
      display: flex; align-items: center; gap: var(--oro-space-3);
      padding: var(--oro-space-3) var(--oro-space-4);
      color: var(--oro-text-secondary); text-decoration: none;
      border-radius: var(--oro-radius); margin: 1px var(--oro-space-2);
      transition: all var(--oro-transition);
    }
    .sidebar-link:hover { background: var(--oro-bg); color: var(--oro-text-primary); }
    .sidebar-link.active { background: color-mix(in srgb, var(--oro-primary) 10%, transparent); color: var(--oro-primary); font-weight: 500; }
    .sidebar-icon { font-size: 1.1rem; flex-shrink: 0; width: 24px; text-align: center; }
    .sidebar-label { white-space: nowrap; font-size: var(--oro-text-sm); }
    .sidebar-toggle {
      margin: var(--oro-space-2); padding: var(--oro-space-2);
      font-size: var(--oro-text-xs); align-self: flex-end;
    }
  `]
})
export class SidebarComponent implements OnInit {
  readonly layout = inject(LayoutService);
  private readonly settingsStore = inject(SettingsStore);
  private readonly sidebarToggle$ = new Subject<boolean>();

  readonly navItems: NavItem[] = [
    { icon: '📊', labelKey: 'MENU.DASHBOARD', route: '/dashboard' },
    { icon: '👥', labelKey: 'MENU.USERS', route: '/users' },
    { icon: '🔑', labelKey: 'MENU.ROLES', route: '/roles' },
    { icon: '🛡️', labelKey: 'MENU.PERMISSIONS', route: '/permissions' },
    { icon: '📱', labelKey: 'MENU.APPLICATIONS', route: '/applications' },
    { icon: '📋', labelKey: 'MENU.SCOPES', route: '/scopes' },
    { icon: '⚙️', labelKey: 'MENU.SETTINGS', route: '/settings' },
  ];

  ngOnInit(): void {
    this.sidebarToggle$.pipe(debounceTime(500)).subscribe(collapsed => {
      this.settingsStore.updatePreference('sidebarCollapsed' as any, collapsed);
    });
  }

  onToggle(): void {
    this.layout.toggleSidebar();
    this.sidebarToggle$.next(this.layout.sidebarCollapsed());
  }
}
