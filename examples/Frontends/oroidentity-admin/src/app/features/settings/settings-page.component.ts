import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationPreferenceGridComponent } from '../notifications/notification-preference-grid.component';
import { SettingsStore } from './settings.store';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [FormsModule, NotificationPreferenceGridComponent, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ 'SETTINGS.TITLE' | translate }}</h2>
      @if (store.saving()) {
        <div class="flex items-center gap-1 text-muted text-sm">
          <div class="oro-spinner" style="width:14px;height:14px;border-width:2px;"></div>
          {{ 'COMMON.SAVING' | translate }}
        </div>
      }
    </div>

    <div class="settings-grid">
      <!-- Appearance -->
      <section class="settings-section oro-card">
        <h3 class="section-title">{{ 'SETTINGS.APPEARANCE' | translate }}</h3>
        <div class="setting-item">
          <label>{{ 'SETTINGS.THEME' | translate }}</label>
          <div class="flex gap-2">
            @for (opt of themes; track opt.value) {
              <button class="theme-chip" [class.active]="store.prefs().theme === opt.value"
                      (click)="update('theme', opt.value)">
                {{ opt.label }}
              </button>
            }
          </div>
        </div>
        <div class="setting-item">
          <label>{{ 'SETTINGS.LANGUAGE' | translate }}</label>
          <select class="oro-input" style="width:200px;" [ngModel]="store.prefs().language"
                  (ngModelChange)="update('language', $event)">
            <option value="es">Español</option>
            <option value="en">English</option>
          </select>
        </div>
        <div class="setting-item">
          <label>{{ 'SETTINGS.CURRENCY' | translate }}</label>
          <select class="oro-input" style="width:200px;" [ngModel]="store.prefs().currency"
                  (ngModelChange)="update('currency', $event)">
            <option value="COP">COP – Peso colombiano</option>
            <option value="MXN">MXN – Peso mexicano</option>
            <option value="PEN">PEN – Sol peruano</option>
            <option value="USD">USD – Dólar americano</option>
          </select>
        </div>
      </section>

      <!-- Notifications -->
      <section class="settings-section oro-card">
        <h3 class="section-title">{{ 'SETTINGS.NOTIFICATIONS' | translate }}</h3>
        <div class="setting-item toggle-item">
          <label>{{ 'SETTINGS.EMAIL_NOTIFICATIONS' | translate }}</label>
          <button class="oro-toggle" [class.on]="store.prefs().emailNotifications"
                  (click)="update('emailNotifications', !store.prefs().emailNotifications)"
                  role="switch" [attr.aria-checked]="store.prefs().emailNotifications">
            <span class="toggle-thumb"></span>
          </button>
        </div>
        <div class="setting-item toggle-item">
          <label>{{ 'SETTINGS.PUSH_NOTIFICATIONS' | translate }}</label>
          <button class="oro-toggle" [class.on]="store.prefs().pushNotifications"
                  (click)="update('pushNotifications', !store.prefs().pushNotifications)"
                  role="switch" [attr.aria-checked]="store.prefs().pushNotifications">
            <span class="toggle-thumb"></span>
          </button>
        </div>
        <div class="setting-item toggle-item">
          <label>{{ 'SETTINGS.DAILY_SUMMARY' | translate }}</label>
          <button class="oro-toggle" [class.on]="store.prefs().dailySummaryEmail"
                  (click)="update('dailySummaryEmail', !store.prefs().dailySummaryEmail)"
                  role="switch" [attr.aria-checked]="store.prefs().dailySummaryEmail">
            <span class="toggle-thumb"></span>
          </button>
        </div>
      </section>

      <!-- Notification Preferences by category -->
      <section class="settings-section oro-card" style="grid-column: 1 / -1;">
        <h3 class="section-title">{{ 'NOTIFICATIONS.PREFERENCES' | translate }}</h3>
        <app-notification-preference-grid />
      </section>

      <!-- AI -->
      <section class="settings-section oro-card">
        <h3 class="section-title">{{ 'SETTINGS.AI_SETTINGS' | translate }}</h3>
        <div class="setting-item">
          <label>{{ 'SETTINGS.AI_CONFIDENCE' | translate }}</label>
          <div class="flex items-center gap-2">
            <input type="range" min="50" max="99" step="1"
                   [ngModel]="store.prefs().aiConfidenceThreshold"
                   (ngModelChange)="update('aiConfidenceThreshold', $event)"
                   style="width:160px;" />
            <strong>{{ store.prefs().aiConfidenceThreshold }}%</strong>
          </div>
        </div>
        <div class="setting-item toggle-item">
          <label>{{ 'SETTINGS.AUTO_APPROVE' | translate }}</label>
          <button class="oro-toggle" [class.on]="store.prefs().autoApproveHighConfidence"
                  (click)="update('autoApproveHighConfidence', !store.prefs().autoApproveHighConfidence)"
                  role="switch" [attr.aria-checked]="store.prefs().autoApproveHighConfidence">
            <span class="toggle-thumb"></span>
          </button>
        </div>
      </section>

      <!-- Fiscal -->
      <section class="settings-section oro-card">
        <h3 class="section-title">{{ 'SETTINGS.FISCAL' | translate }}</h3>
        <div class="setting-item">
          <label>{{ 'SETTINGS.FISCAL_YEAR_START' | translate }}</label>
          <select class="oro-input" style="width:200px;" [ngModel]="store.prefs().fiscalYearStart"
                  (ngModelChange)="update('fiscalYearStart', +$event)">
            @for (m of months; track m.value) {
              <option [value]="m.value">{{ m.label }}</option>
            }
          </select>
        </div>
        <div class="setting-item">
          <label>{{ 'SETTINGS.DATE_FORMAT' | translate }}</label>
          <select class="oro-input" style="width:200px;" [ngModel]="store.prefs().dateFormat"
                  (ngModelChange)="update('dateFormat', $event)">
            <option value="dd/MM/yyyy">DD/MM/AAAA</option>
            <option value="MM/dd/yyyy">MM/DD/AAAA</option>
            <option value="yyyy-MM-dd">AAAA-MM-DD</option>
          </select>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .page-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
    .page-title { font-size: var(--oro-text-2xl); font-weight: 700; flex: 1; }
    .settings-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(340px, 1fr)); gap: 1rem; }
    .settings-section { padding: 1.25rem; }
    .section-title { font-size: var(--oro-text-lg); font-weight: 600; margin-bottom: 1rem; }
    .setting-item { margin-bottom: 1rem; }
    .setting-item label { display: block; font-size: var(--oro-text-sm); font-weight: 500; margin-bottom: .4rem; color: var(--oro-text-secondary); }
    .toggle-item { display: flex; align-items: center; justify-content: space-between; }
    .toggle-item label { margin-bottom: 0; }
    .theme-chip { padding: .3rem .75rem; border: 1px solid var(--oro-border); border-radius: var(--oro-radius); background: var(--oro-bg); cursor: pointer; font-size: var(--oro-text-sm); transition: all var(--oro-transition); }
    .theme-chip.active { background: var(--oro-primary); color: white; border-color: var(--oro-primary); }
    .oro-toggle { width: 44px; height: 24px; border-radius: 999px; background: var(--oro-border); border: none; cursor: pointer; position: relative; transition: background var(--oro-transition); }
    .oro-toggle.on { background: var(--oro-primary); }
    .toggle-thumb { position: absolute; top: 2px; left: 2px; width: 20px; height: 20px; border-radius: 50%; background: white; transition: transform var(--oro-transition); box-shadow: var(--oro-shadow-sm); }
    .oro-toggle.on .toggle-thumb { transform: translateX(20px); }
  `]
})
export class SettingsPageComponent implements OnInit {
  readonly store = inject(SettingsStore);
  readonly profile = inject(AuthService).userProfile();

  themes = [
    { value: 'light', label: '☀️ Claro' },
    { value: 'dark', label: '🌙 Oscuro' },
    { value: 'system', label: '💻 Sistema' }
  ];

  months = [
    { value: 1, label: 'Enero' }, { value: 2, label: 'Febrero' }, { value: 3, label: 'Marzo' },
    { value: 4, label: 'Abril' }, { value: 5, label: 'Mayo' }, { value: 6, label: 'Junio' },
    { value: 7, label: 'Julio' }, { value: 8, label: 'Agosto' }, { value: 9, label: 'Septiembre' },
    { value: 10, label: 'Octubre' }, { value: 11, label: 'Noviembre' }, { value: 12, label: 'Diciembre' }
  ];

  ngOnInit(): void {
    this.store.loadPreferences(this.profile?.sub as string, this.profile?.tenantId as string);
  }

  update(field: string, value: unknown): void {
    this.store.updatePreference(field as any, value);
  }
}
