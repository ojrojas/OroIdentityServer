import { Component, inject, OnInit } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { NotificationStore } from './notification.store';
import { TimeAgoPipe } from '../../shared/pipes/time-ago.pipe';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [],
  template: `
    <div class="bell-wrapper" (click)="toggle()" (keydown.enter)="toggle()" tabindex="0" role="button" [attr.aria-label]="'NOTIFICATIONS.TITLE' | translate">
      <button class="oro-btn oro-btn-ghost bell-btn">
        <span class="bell-icon">🔔</span>
        @if (store.unreadCount() > 0) {
          <span class="badge">{{ store.unreadCount() > 99 ? '99+' : store.unreadCount() }}</span>
        }
      </button>
    </div>
  `,
  styles: [`
    .bell-wrapper { position: relative; display: inline-flex; }
    .bell-btn { position: relative; }
    .bell-icon { font-size: 1.2rem; }
    .badge {
      position: absolute; top: 0; right: 0;
      background: var(--oro-danger); color: white;
      font-size: .6rem; font-weight: 700;
      border-radius: 999px; padding: .05rem .3rem;
      min-width: 1rem; text-align: center;
      border: 2px solid var(--oro-surface);
    }
  `]
})
export class NotificationBellComponent {
  readonly store = inject(NotificationStore);

  toggle(): void {
    // The parent header component manages the dropdown state
    this.store.load();
  }
}

@Component({
  selector: 'app-notification-dropdown',
  standalone: true,
  imports: [TimeAgoPipe, TranslatePipe],
  template: `
    <div class="notif-dropdown">
      <div class="notif-header">
        <span class="font-semibold">{{ 'NOTIFICATIONS.TITLE' | translate }}</span>
        @if (store.unreadCount() > 0) {
          <button class="oro-btn oro-btn-ghost" style="font-size:var(--oro-text-xs);" (click)="store.markAllAsRead()">
            {{ 'NOTIFICATIONS.MARK_ALL_READ' | translate }}
          </button>
        }
      </div>
      <div class="notif-list">
        @if (!store.notifications().length) {
          <div class="notif-empty">
            <span>🔕</span><p class="text-muted text-sm">{{ 'NOTIFICATIONS.NO_NOTIFICATIONS' | translate }}</p>
          </div>
        }
        @for (notif of store.notifications(); track notif.id) {
          <div class="notif-item" [class.unread]="!notif.isRead" (click)="store.markAsRead(notif.id)">
            <span class="notif-icon">{{ getIcon(notif.type) }}</span>
            <div class="notif-body">
              <p class="notif-title">{{ notif.title }}</p>
              <p class="text-muted text-sm">{{ notif.message }}</p>
              <span class="text-muted" style="font-size:.7rem;">{{ notif.createdAt | timeAgo }}</span>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .notif-dropdown { width: 320px; background: var(--oro-surface); border: 1px solid var(--oro-border); border-radius: var(--oro-radius-lg); box-shadow: var(--oro-shadow-lg); overflow: hidden; }
    .notif-header { display: flex; align-items: center; justify-content: space-between; padding: .75rem 1rem; border-bottom: 1px solid var(--oro-border); font-size: var(--oro-text-sm); }
    .notif-list { max-height: 400px; overflow-y: auto; }
    .notif-item { display: flex; gap: .75rem; padding: .75rem 1rem; border-bottom: 1px solid var(--oro-border); cursor: pointer; transition: background var(--oro-transition); }
    .notif-item:hover { background: var(--oro-bg); }
    .notif-item.unread { background: color-mix(in srgb, var(--oro-primary) 6%, transparent); }
    .notif-icon { font-size: 1.4rem; }
    .notif-body { flex: 1; min-width: 0; }
    .notif-title { font-size: var(--oro-text-sm); font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .notif-empty { display: flex; flex-direction: column; align-items: center; justify-content: center; gap: .5rem; padding: 2rem; font-size: 2rem; }
  `]
})
export class NotificationDropdownComponent implements OnInit {
  readonly store = inject(NotificationStore);

  ngOnInit(): void { this.store.load(); }

  getIcon(type: string): string {
    const map: Record<string, string> = { Info: 'ℹ️', Warning: '⚠️', Error: '❌', Success: '✅', AI: '🤖' };
    return map[type] ?? '🔔';
  }
}
