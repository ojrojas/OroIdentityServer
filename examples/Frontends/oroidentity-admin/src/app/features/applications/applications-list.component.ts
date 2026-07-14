import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastService } from '../../shared/toast.service';
import { ApplicationsStore } from './applications.store';

@Component({
  selector: 'app-applications-list',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ 'APPLICATIONS.TITLE' | translate }}</h2>
      <a class="oro-btn oro-btn-primary" routerLink="/applications/new">{{ 'APPLICATIONS.NEW' | translate }}</a>
    </div>

    @if (store.loading()) {
      <div class="flex items-center gap-2 p-4"><div class="oro-spinner"></div></div>
    } @else {
      <div class="oro-card table-wrapper">
        <table class="oro-table">
          <thead>
            <tr>
              <th>{{ 'APPLICATIONS.CLIENT_ID' | translate }}</th>
              <th>{{ 'APPLICATIONS.DISPLAY_NAME' | translate }}</th>
              <th>{{ 'APPLICATIONS.CLIENT_TYPE' | translate }}</th>
              <th>{{ 'APPLICATIONS.APPLICATION_TYPE' | translate }}</th>
              <th>{{ 'COMMON.ACTIONS' | translate }}</th>
            </tr>
          </thead>
          <tbody>
            @for (app of store.applications(); track app.clientId) {
              <tr>
                <td><code>{{ app.clientId }}</code></td>
                <td>{{ app.displayName }}</td>
                <td>{{ app.clientType }}</td>
                <td>{{ app.applicationType }}</td>
                <td class="actions-cell">
                  <a class="oro-btn oro-btn-ghost" [routerLink]="['/applications', app.clientId, 'edit']">{{ 'COMMON.EDIT' | translate }}</a>
                  <button class="oro-btn oro-btn-ghost oro-btn-danger" (click)="onDelete(app)">{{ 'COMMON.DELETE' | translate }}</button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="empty-row">{{ 'COMMON.NO_RESULTS' | translate }}</td></tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
  styles: [`
    .page-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
    .page-title { font-size: var(--oro-text-2xl); font-weight: 700; flex: 1; }
    .table-wrapper { padding: 0; overflow-x: auto; }
    .oro-table { width: 100%; border-collapse: collapse; font-size: var(--oro-text-sm); }
    .oro-table th, .oro-table td { padding: 0.75rem 1rem; border-bottom: 1px solid var(--oro-border); text-align: left; }
    .oro-table th { font-weight: 600; color: var(--oro-text-secondary); background: var(--oro-bg); }
    .actions-cell { white-space: nowrap; }
    .empty-row { text-align: center; padding: 2rem !important; color: var(--oro-text-secondary); }
    .oro-btn-danger { color: var(--oro-danger); }
    code { background: var(--oro-bg); padding: 0.1rem 0.3rem; border-radius: 4px; font-size: 0.85em; }
  `]
})
export class ApplicationsListComponent implements OnInit {
  readonly store = inject(ApplicationsStore);
  private readonly toast = inject(ToastService);

  ngOnInit(): void { this.store.load(); }

  onDelete(app: { clientId: string; displayName?: string }): void {
    if (confirm(`Delete application ${app.displayName ?? app.clientId}?`)) {
      this.store.delete(app.clientId);
      this.toast.show('APPLICATIONS.DELETED');
    }
  }
}
