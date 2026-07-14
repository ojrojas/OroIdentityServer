import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { TimeAgoPipe } from '../../shared/pipes/time-ago.pipe';
import { NotificationStore } from './notification.store';

@Component({
  selector: 'app-notification-full-page',
  standalone: true,
  imports: [FormsModule, TimeAgoPipe, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ 'NOTIFICATIONS.TITLE' | translate }}</h2>
      <div class="flex items-center gap-2">
        <select class="oro-input" [(ngModel)]="filter" (change)="applyFilter()" style="width:160px;">
          <option value="all">All</option>
          <option value="unread">Unread</option>
        </select>
        <button class="oro-btn oro-btn-secondary" (click)="store.markAllAsRead()">{{ 'NOTIFICATIONS.MARK_ALL_READ' | translate }}</button>
      </div>
    </div>

    @if (store.loading()) {
      <div class="flex items-center gap-2"><div class="oro-spinner"></div></div>
    } @else {
      <div class="notif-timeline">
        @for (notif of displayedNotifications(); track notif.id) {
          <div class="timeline-item" [class.unread]="!notif.isRead">
            <div class="timeline-dot" [class]="'dot-' + notif.type.toLowerCase()">
              {{ getIcon(notif.type) }}
            </div>
            <div class="timeline-content oro-card">
              <div class="flex justify-between items-start">
                <div>
                  <span class="font-semibold">{{ notif.title }}</span>
                  <span class="oro-badge oro-badge-info" style="margin-left:.5rem; font-size:.65rem;">{{ notif.type }}</span>
                </div>
                <span class="text-muted" style="font-size:.7rem; white-space:nowrap; margin-left:.5rem;">{{ notif.createdAt | timeAgo }}</span>
              </div>
              <p class="text-muted text-sm" style="margin-top:.25rem;">{{ notif.message }}</p>
              @if (!notif.isRead) {
                <button class="oro-btn oro-btn-ghost" style="font-size:var(--oro-text-xs); margin-top:.5rem;" (click)="store.markAsRead(notif.id)">
                  Mark done
                </button>
              }
            </div>
          </div>
        }
        @if (!displayedNotifications().length) {
          <div style="text-align:center;padding:3rem;color:var(--oro-text-secondary);">
            <div style="font-size:3rem;">🔕</div>
            <p>notifications{{ filter === 'unread' ? ' sin leer' : '' }}</p>
          </div>
        }
      </div>
    }
  `,
  styles: [`
    .page-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1rem; flex-wrap: wrap; }
    .page-title { font-size: var(--oro-text-2xl); font-weight: 700; flex: 1; }
    .notif-timeline { display: flex; flex-direction: column; gap: 1rem; max-width: 700px; }
    .timeline-item { display: flex; gap: 1rem; align-items: flex-start; }
    .timeline-item.unread .timeline-content { border-left: 3px solid var(--oro-primary); }
    .timeline-dot { width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center; flex-shrink: 0; font-size: 1rem; background: var(--oro-bg); border: 2px solid var(--oro-border); }
    .dot-warning { border-color: var(--oro-warning); }
    .dot-error { border-color: var(--oro-danger); }
    .dot-success { border-color: var(--oro-success); }
    .dot-ai { border-color: var(--oro-primary); }
    .timeline-content { flex: 1; padding: .75rem 1rem; }
  `]
})
export class NotificationFullPageComponent implements OnInit {
  readonly store = inject(NotificationStore);
  filter: 'all' | 'unread' = 'all';

  ngOnInit(): void { this.store.load(); }

  displayedNotifications() {
    return this.filter === 'unread'
      ? this.store.notifications().filter(n => !n.isRead)
      : this.store.notifications();
  }

  applyFilter(): void {}

  getIcon(type: string): string {
    const map: Record<string, string> = { Info: 'ℹ️', Warning: '⚠️', Error: '❌', Success: '✅', AI: '🤖' };
    return map[type] ?? '🔔';
  }
}
