import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastService } from '../../shared/toast.service';
import { RolesStore } from './roles.store';

@Component({
  selector: 'app-roles-list',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ 'ROLES.TITLE' | translate }}</h2>
      <a class="oro-btn oro-btn-primary" routerLink="/roles/new">{{ 'ROLES.NEW' | translate }}</a>
    </div>

    @if (store.loading()) {
      <div class="flex items-center gap-2 p-4"><div class="oro-spinner"></div></div>
    } @else {
      <div class="oro-card table-wrapper">
        <table class="oro-table">
          <thead>
            <tr>
              <th>{{ 'ROLES.NAME' | translate }}</th>
              <th>{{ 'ROLES.IS_ACTIVE' | translate }}</th>
              <th>{{ 'ROLES.PERMISSIONS' | translate }}</th>
              <th>{{ 'COMMON.ACTIONS' | translate }}</th>
            </tr>
          </thead>
          <tbody>
            @for (role of store.roles(); track role.id) {
              <tr>
                <td>{{ role.name }}</td>
                <td>
                  <span class="oro-badge" [class.oro-badge-success]="role.isActive" [class.oro-badge-muted]="!role.isActive">
                    {{ (role.isActive ? 'COMMON.CONFIRM' : 'COMMON.CANCEL') | translate }}
                  </span>
                </td>
                <td>{{ role.claims?.length ?? 0 }}</td>
                <td class="actions-cell">
                  <a class="oro-btn oro-btn-ghost" [routerLink]="['/roles', role.id, 'edit']">{{ 'COMMON.EDIT' | translate }}</a>
                  <button class="oro-btn oro-btn-ghost oro-btn-danger" (click)="onDelete(role)">{{ 'COMMON.DELETE' | translate }}</button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="4" class="empty-row">{{ 'COMMON.NO_RESULTS' | translate }}</td></tr>
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
  `]
})
export class RolesListComponent implements OnInit {
  readonly store = inject(RolesStore);
  private readonly toast = inject(ToastService);

  ngOnInit(): void { this.store.load(); }

  onDelete(role: { id: string; name: string }): void {
    if (confirm(`Delete role ${role.name}?`)) {
      this.store.delete(role.id);
      this.toast.show('ROLES.DELETED');
    }
  }
}
