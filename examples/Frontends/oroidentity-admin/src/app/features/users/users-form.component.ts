import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastService } from '../../shared/toast.service';
import { UsersStore } from './users.store';
import { CreateUserCommand, UpdateUserCommand } from '../../core/models';

@Component({
  selector: 'app-users-form',
  standalone: true,
  imports: [FormsModule, RouterLink, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ isEdit() ? ('USERS.EDIT' | translate) : ('USERS.NEW' | translate) }}</h2>
      <a class="oro-btn oro-btn-ghost" routerLink="/users">{{ 'COMMON.BACK' | translate }}</a>
    </div>

    <div class="oro-card form-card">
      <form #f="ngForm" (ngSubmit)="onSubmit()" class="form-grid">
        <div class="form-group">
          <label>{{ 'USERS.NAME' | translate }}</label>
          <input class="oro-input" name="name" [(ngModel)]="model.name" required />
        </div>
        <div class="form-group">
          <label>{{ 'USERS.MIDDLE_NAME' | translate }}</label>
          <input class="oro-input" name="middleName" [(ngModel)]="model.middleName" />
        </div>
        <div class="form-group">
          <label>{{ 'USERS.LAST_NAME' | translate }}</label>
          <input class="oro-input" name="lastName" [(ngModel)]="model.lastName" required />
        </div>
        <div class="form-group">
          <label>{{ 'USERS.USERNAME' | translate }}</label>
          <input class="oro-input" name="userName" [(ngModel)]="model.userName" required />
        </div>
        <div class="form-group">
          <label>{{ 'USERS.EMAIL' | translate }}</label>
          <input class="oro-input" name="email" type="email" [(ngModel)]="model.email" required />
        </div>
        @if (!isEdit()) {
          <div class="form-group">
            <label>{{ 'USERS.PASSWORD' | translate }}</label>
            <input class="oro-input" name="password" type="password" [(ngModel)]="password" required />
          </div>
        }
        <div class="form-group">
          <label>{{ 'USERS.IDENTIFICATION' | translate }}</label>
          <input class="oro-input" name="identification" [(ngModel)]="model.identification" required />
        </div>
        <div class="form-actions">
          <button class="oro-btn oro-btn-primary" type="submit" [disabled]="f.invalid">{{ 'COMMON.SAVE' | translate }}</button>
          <a class="oro-btn oro-btn-ghost" routerLink="/users">{{ 'COMMON.CANCEL' | translate }}</a>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .page-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
    .page-title { font-size: var(--oro-text-2xl); font-weight: 700; flex: 1; }
    .form-card { padding: 1.5rem; max-width: 640px; }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.25rem; }
    .form-group label { font-size: var(--oro-text-sm); font-weight: 500; color: var(--oro-text-secondary); }
    .form-actions { grid-column: 1 / -1; display: flex; gap: 0.75rem; padding-top: 0.5rem; }
  `]
})
export class UsersFormComponent implements OnInit {
  private readonly store = inject(UsersStore);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isEdit = signal(false);
  readonly password = signal('');

  model: CreateUserCommand = {
    name: '', middleName: '', lastName: '', userName: '', email: '',
    password: '', identification: '', identificationTypeId: '', tenantId: '',
  };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      const user = this.store.users().find(u => u.id === id);
      if (user) {
        this.model = {
          name: user.name, middleName: user.middleName ?? '', lastName: user.lastName,
          userName: user.userName, email: user.email, password: '',
          identification: user.identification, identificationTypeId: user.identificationTypeId,
          tenantId: user.tenantId,
        };
      }
    }
  }

  onSubmit(): void {
    if (this.isEdit()) {
      const id = this.route.snapshot.paramMap.get('id')!;
      this.store.update(id, this.model as any);
      this.toast.show('USERS.UPDATED', 'success');
    } else {
      this.store.create({ ...this.model, password: this.password() });
      this.toast.show('USERS.CREATED', 'success');
    }
    this.router.navigate(['/users']);
  }
}
