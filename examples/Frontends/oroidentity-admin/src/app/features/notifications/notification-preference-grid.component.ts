import { Component, inject, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface NotificationPref {
  category: string;
  channelEmail: boolean;
  channelPush: boolean;
  channelInApp: boolean;
}

@Component({
  selector: 'app-notification-preference-grid',
  standalone: true,
  template: `
    <div class="notif-pref-grid">
      <table class="oro-table" style="width:100%;">
        <thead>
          <tr>
            <th>Categoría</th>
            <th style="text-align:center">📧 Email</th>
            <th style="text-align:center">📱 Push</th>
            <th style="text-align:center">🔔 En App</th>
          </tr>
        </thead>
        <tbody>
          @for (pref of prefs(); track pref.category) {
            <tr>
              <td class="category-cell">{{ categoryLabel(pref.category) }}</td>
              <td style="text-align:center">
                <button class="oro-toggle" [class.on]="pref.channelEmail"
                        (click)="toggle(pref, 'channelEmail')"
                        role="switch" [attr.aria-checked]="pref.channelEmail">
                  <span class="toggle-thumb"></span>
                </button>
              </td>
              <td style="text-align:center">
                <button class="oro-toggle" [class.on]="pref.channelPush"
                        (click)="toggle(pref, 'channelPush')"
                        role="switch" [attr.aria-checked]="pref.channelPush">
                  <span class="toggle-thumb"></span>
                </button>
              </td>
              <td style="text-align:center">
                <button class="oro-toggle" [class.on]="pref.channelInApp"
                        (click)="toggle(pref, 'channelInApp')"
                        role="switch" [attr.aria-checked]="pref.channelInApp">
                  <span class="toggle-thumb"></span>
                </button>
              </td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .notif-pref-grid { overflow-x: auto; }
    .oro-table { border-collapse: collapse; font-size: var(--oro-text-sm); }
    .oro-table th, .oro-table td { padding: 0.6rem 1rem; border-bottom: 1px solid var(--oro-border); }
    .oro-table th { font-weight: 600; color: var(--oro-text-secondary); text-align: left; }
    .category-cell { font-weight: 500; }
    .oro-toggle { width: 44px; height: 24px; border-radius: 999px; background: var(--oro-border); border: none; cursor: pointer; position: relative; transition: background var(--oro-transition); }
    .oro-toggle.on { background: var(--oro-primary); }
    .toggle-thumb { position: absolute; top: 2px; left: 2px; width: 20px; height: 20px; border-radius: 50%; background: white; transition: transform var(--oro-transition); box-shadow: var(--oro-shadow-sm); display: block; }
    .oro-toggle.on .toggle-thumb { transform: translateX(20px); }
  `]
})
export class NotificationPreferenceGridComponent implements OnInit {
  private readonly http = inject(HttpClient);

  readonly prefs = signal<NotificationPref[]>([
    { category: 'Inbox', channelEmail: true, channelPush: true, channelInApp: true },
    { category: 'Documents', channelEmail: true, channelPush: false, channelInApp: true },
    { category: 'Accounting', channelEmail: true, channelPush: false, channelInApp: true },
    { category: 'AI', channelEmail: false, channelPush: false, channelInApp: true },
    { category: 'System', channelEmail: true, channelPush: true, channelInApp: true },
  ]);

  ngOnInit(): void {
    this.http.get<NotificationPref[]>('/api/notifications/preferences')
      .subscribe({ next: data => { if (data?.length) this.prefs.set(data); } });
  }

  categoryLabel(category: string): string {
    const labels: Record<string, string> = {
      Inbox: '📥 Inbox', Documents: '📄 Documentos',
      Accounting: '📖 Contabilidad', AI: '🤖 IA', System: '⚙️ Sistema'
    };
    return labels[category] ?? category;
  }

  toggle(pref: NotificationPref, channel: keyof Pick<NotificationPref, 'channelEmail' | 'channelPush' | 'channelInApp'>): void {
    const updated = this.prefs().map(p =>
      p.category === pref.category ? { ...p, [channel]: !p[channel] } : p
    );
    this.prefs.set(updated);
  }
}
