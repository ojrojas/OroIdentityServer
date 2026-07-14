import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastService } from '../../shared/toast.service';
import { UsersStore } from './users.store';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ 'USERS.TITLE' | translate }}</h2>
      <a class="oro-btn oro-btn-primary" routerLink="/users/new">{{ 'USERS.NEW' | translate }}</a>
    </div>

    @if (store.loading()) {
      <div class="flex items-center gap-2 p-4"><div class="oro-spinner"></div></div>
    } @else {
      <div class="oro-card table-wrapper">
        <table class="oro-table">
          <thead>
            <tr>
              <th>{{ 'USERS.NAME' | translate }}</th>
              <th>{{ 'USERS.LAST_NAME' | translate }}</th>
              <th>{{ 'USERS.USERNAME' | translate }}</th>
              <th>{{ 'USERS.EMAIL' | translate }}</th>
              <th>{{ 'USERS.IDENTIFICATION' | translate }}</th>
              <th>{{ 'COMMON.ACTIONS' | translate }}</th>
            </tr>
          </thead>
          <tbody>
            @for (user of store.users(); track user.id) {
              <tr>
                <td>{{ user.name }}</td>
                <td>{{ user.lastName }}</td>
                <td>{{ user.userName }}</td>
                <td>{{ user.email }}</td>
                <td>{{ user.identification }}</td>
                <td class="actions-cell">
                  <a class="oro-btn oro-btn-ghost" [routerLink]="['/users', user.id, 'edit']">{{ 'COMMON.EDIT' | translate }}</a>
                  <button class="oro-btn oro-btn-ghost oro-btn-danger" (click)="onDelete(user)">{{ 'COMMON.DELETE' | translate }}</button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="6" class="empty-row">{{ 'COMMON.NO_RESULTS' | translate }}</td></tr>
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
export class UsersListComponent implements OnInit {
  readonly store = inject(UsersStore);
  private readonly toast = inject(ToastService);

  ngOnInit(): void { this.store.load(); }

  onDelete(user: { id: string; name: string }): void {
    if (confirm(`Delete user ${user.name}?`)) {
      this.store.delete(user.id);
      this.toast.show('USERS.DELETED');
    }
  }
}
