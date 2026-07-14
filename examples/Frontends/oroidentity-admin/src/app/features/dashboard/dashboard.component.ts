import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { KpiData } from '../../core/models';
import { KpiCardComponent } from '../../shared/components/kpi-card/kpi-card.component';
import { CompanyStore } from '../company/company.store';
import { DashboardLayoutStore } from './dashboard-layout.store';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, KpiCardComponent, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ 'DASHBOARD.TITLE' | translate }}</h2>
      <span class="text-muted text-sm">{{ store.activeCompanyName() }}</span>
    </div>

    @if (loading()) {
      <div class="flex items-center gap-2" style="padding: 2rem;">
        <div class="oro-spinner"></div>
        <span class="text-muted">{{ 'COMMON.LOADING' | translate }}</span>
      </div>
    } @else {
      <div class="kpi-grid" >
        @for (kpi of kpis(); track kpi.label) {
          <app-kpi-card [kpi]="kpi"  />
        }
      </div>

      <div class="dashboard-alerts oro-card" style="margin-top: 1.5rem;">
        <h3 class="section-title">{{ 'DASHBOARD.ALERTS' | translate }}</h3>
        @if (alerts().length === 0) {
          <p class="text-muted text-sm" style="padding: 1rem 0;">{{ 'DASHBOARD.NO_ALERTS' | translate }}</p>
        } @else {
          <ul class="alert-list">
            @for (alert of alerts(); track $index) {
              <li class="alert-item">
                <span class="oro-badge oro-badge-warning">{{ alert.type }}</span>
                <span class="text-sm">{{ alert.message }}</span>
                <a [routerLink]="alert.route" class="oro-btn oro-btn-ghost" style="font-size:0.75rem;">Ver →</a>
              </li>
            }
          </ul>
        }
      </div>
    }
  `,
  styles: [`
    .page-header { display: flex; align-items: baseline; gap: 1rem; margin-bottom: 1.5rem; }
    .page-title { font-size: var(--oro-text-2xl); font-weight: 700; }
    .kpi-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1rem; }
    .section-title { font-size: var(--oro-text-lg); font-weight: 600; margin-bottom: 0.75rem; }
    .alert-list { list-style: none; display: flex; flex-direction: column; gap: 0.5rem; }
    .alert-item { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0; border-bottom: 1px solid var(--oro-border); }
    .alert-item:last-child { border-bottom: none; }
    .cdk-drag-preview { opacity: 0.8; box-shadow: var(--oro-shadow-lg); }
    .cdk-drag-placeholder { opacity: 0.3; }
    .cdk-drag-animating { transition: transform 250ms cubic-bezier(0, 0, 0.2, 1); }
  `]
})
export class DashboardComponent implements OnInit {
  readonly store = inject(CompanyStore);
  readonly layoutStore = inject(DashboardLayoutStore);
  private readonly http = inject(HttpClient);

  readonly loading = signal(true);
  readonly kpis = signal<KpiData[]>([]);
  readonly alerts = signal<{ type: string; message: string; route: string }[]>([]);

  ngOnInit(): void {
    const companyId = this.store.activeCompany()?.id;
    if (!companyId) { this.loading.set(false); return; }
    this.http.get<{ kpis: KpiData[]; alerts: { type: string; message: string; route: string }[] }>(
      `/api/dashboard/${companyId}`
    ).subscribe({
      next: data => { this.kpis.set(data.kpis); this.alerts.set(data.alerts); },
      error: () => {
        this.kpis.set([
          { label: 'Documentos Pendientes', value: 0, icon: '📄', route: '/documents' },
          { label: 'Asientos por Revisar', value: 0, icon: '📖', route: '/journal-entries' },
          { label: 'Tareas en Inbox', value: 0, icon: '📥', route: '/inbox' },
        ]);
      },
      complete: () => this.loading.set(false),
    });
  }

  onDrop(event: CdkDragDrop<KpiData[]>): void {
    this.layoutStore.moveWidget(event.previousIndex, event.currentIndex);
    const items = [...this.kpis()];
    const [moved] = items.splice(event.previousIndex, 1);
    items.splice(event.currentIndex, 0, moved);
    this.kpis.set(items);
  }
}
