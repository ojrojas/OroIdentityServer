import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { TranslatePipe } from '@ngx-translate/core';

interface TenantPreferenceConfig {
  tenantId: string;
  defaultLanguage: string;
  defaultTimezone: string;
  defaultDateFormat: string;
  defaultNumberFormat: string;
  defaultTheme: string;
  forceLanguage: boolean;
  forceTheme: boolean;
}

@Component({
  selector: 'app-tenant-settings-page',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ 'TENANT_SETTINGS.TITLE' | translate }}</h2>
      @if (saving()) {
        <div class="flex items-center gap-1 text-muted text-sm">
          <div class="oro-spinner" style="width:14px;height:14px;border-width:2px;"></div>
          {{ 'COMMON.SAVING' | translate }}
        </div>
      }
    </div>

    @if (config()) {
      <div class="settings-grid">
        <section class="settings-section oro-card">
          <h3 class="section-title">{{ 'TENANT_SETTINGS.DEFAULT_VALUES' | translate }}</h3>
 
          <div class="setting-item">
            <label>{{ 'TENANT_SETTINGS.DEFAULT_LANGUAGE' | translate }}</label>
            <select class="oro-input" style="width:200px;" [(ngModel)]="config()!.defaultLanguage">
              <option value="Es">Español</option>
              <option value="En">English</option>
            </select>
          </div>

          <div class="setting-item">
            <label>{{ 'TENANT_SETTINGS.DEFAULT_TIMEZONE' | translate }}</label>
            <input type="text" class="oro-input" style="width:280px;" [(ngModel)]="config()!.defaultTimezone"
                   placeholder="America/Bogota" />
          </div>

          <div class="setting-item">
            <label>{{ 'TENANT_SETTINGS.DEFAULT_DATE_FORMAT' | translate }}</label>
            <select class="oro-input" style="width:200px;" [(ngModel)]="config()!.defaultDateFormat">
              <option value="DdMmYyyy">DD/MM/AAAA</option>
              <option value="MmDdYyyy">MM/DD/AAAA</option>
              <option value="YyyyMmDd">AAAA-MM-DD</option>
            </select>
          </div>

          <div class="setting-item">
            <label>{{ 'TENANT_SETTINGS.DEFAULT_NUMBER_FORMAT' | translate }}</label>
            <select class="oro-input" style="width:200px;" [(ngModel)]="config()!.defaultNumberFormat">
              <option value="DotDecimal">1,000.00</option>
              <option value="CommaDecimal">1.000,00</option>
            </select>
          </div>

          <div class="setting-item">
            <label>{{ 'TENANT_SETTINGS.DEFAULT_THEME' | translate }}</label>
            <select class="oro-input" style="width:200px;" [(ngModel)]="config()!.defaultTheme">
              <option value="Light">Claro</option>
              <option value="Dark">Oscuro</option>
              <option value="System">Sistema</option>
            </select>
          </div>
        </section>

        <section class="settings-section oro-card">
          <h3 class="section-title">{{ 'TENANT_SETTINGS.POLICIES' | translate }}</h3>
          <div class="setting-item toggle-item">
            <label>{{ 'TENANT_SETTINGS.FORCE_LANGUAGE' | translate }}</label>
            <button class="oro-toggle" [class.on]="config()!.forceLanguage"
                    (click)="config()!.forceLanguage = !config()!.forceLanguage"
                    role="switch" [attr.aria-checked]="config()!.forceLanguage">
              <span class="toggle-thumb"></span>
            </button>
          </div>
          <div class="setting-item toggle-item">
            <label>{{ 'TENANT_SETTINGS.FORCE_THEME' | translate }}</label>
            <button class="oro-toggle" [class.on]="config()!.forceTheme"
                    (click)="config()!.forceTheme = !config()!.forceTheme"
                    role="switch" [attr.aria-checked]="config()!.forceTheme">
              <span class="toggle-thumb"></span>
            </button>
          </div>

          <div style="margin-top: 1.5rem;">
            <button class="oro-btn oro-btn-primary" (click)="save()">{{ 'TENANT_SETTINGS.SAVE_CONFIG' | translate }}</button>
          </div>
        </section>
      </div>
    } @else if (loading()) {
      <div class="flex items-center gap-2" style="padding: 2rem;">
        <div class="oro-spinner"></div>
        <span class="text-muted">{{ 'COMMON.LOADING' | translate }}</span>
      </div>
    }
  `,
  styles: [`
    .page-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
    .page-title { font-size: var(--oro-text-2xl); font-weight: 700; flex: 1; }
    .settings-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(360px, 1fr)); gap: 1rem; }
    .settings-section { padding: 1.25rem; }
    .section-title { font-size: var(--oro-text-lg); font-weight: 600; margin-bottom: 1rem; }
    .setting-item { margin-bottom: 1rem; }
    .setting-item label { display: block; font-size: var(--oro-text-sm); font-weight: 500; margin-bottom: .4rem; color: var(--oro-text-secondary); }
    .toggle-item { display: flex; align-items: center; justify-content: space-between; }
    .toggle-item label { margin-bottom: 0; }
    .oro-toggle { width: 44px; height: 24px; border-radius: 999px; background: var(--oro-border); border: none; cursor: pointer; position: relative; transition: background var(--oro-transition); }
    .oro-toggle.on { background: var(--oro-primary); }
    .toggle-thumb { position: absolute; top: 2px; left: 2px; width: 20px; height: 20px; border-radius: 50%; background: white; transition: transform var(--oro-transition); box-shadow: var(--oro-shadow-sm); display: block; }
    .oro-toggle.on .toggle-thumb { transform: translateX(20px); }
  `]
})
export class TenantSettingsPageComponent implements OnInit {
  private readonly http = inject(HttpClient);

  readonly config = signal<TenantPreferenceConfig | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);

  ngOnInit(): void {
    this.loading.set(true);
    this.http.get<{ data: TenantPreferenceConfig }>('/api/admin/preferences')
      .subscribe({
        next: res => this.config.set(res.data),
        complete: () => this.loading.set(false),
      });
  }

  save(): void {
    const c = this.config();
    if (!c) return;
    this.saving.set(true);
    this.http.put('/api/admin/preferences', c).subscribe({
      complete: () => this.saving.set(false),
    });
  }
}
