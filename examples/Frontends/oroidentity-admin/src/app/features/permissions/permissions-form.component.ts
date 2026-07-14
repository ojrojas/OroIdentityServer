import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastService } from '../../shared/toast.service';
import { PermissionsStore } from './permissions.store';
import { CreatePermissionCommand } from '../../core/models';

@Component({
  selector: 'app-permissions-form',
  standalone: true,
  imports: [FormsModule, RouterLink, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ isEdit() ? ('PERMISSIONS.EDIT' | translate) : ('PERMISSIONS.NEW' | translate) }}</h2>
      <a class="oro-btn oro-btn-ghost" routerLink="/permissions">{{ 'COMMON.BACK' | translate }}</a>
    </div>

    <div class="oro-card form-card">
      <form #f="ngForm" (ngSubmit)="onSubmit()" class="form-grid">
        <div class="form-group">
          <label>{{ 'PERMISSIONS.PROVIDER' | translate }}</label>
          <input class="oro-input" name="provider" [(ngModel)]="model.provider" required />
        </div>
        <div class="form-group">
          <label>{{ 'PERMISSIONS.ACTION' | translate }}</label>
          <input class="oro-input" name="action" [(ngModel)]="model.action" required />
        </div>
        <div class="form-group">
          <label>{{ 'PERMISSIONS.RESOURCE' | translate }}</label>
          <input class="oro-input" name="resource" [(ngModel)]="model.resource" required />
        </div>
        <div class="form-group">
          <label>{{ 'PERMISSIONS.DESCRIPTION' | translate }}</label>
          <input class="oro-input" name="description" [(ngModel)]="model.description" />
        </div>
        <div class="form-group checkbox-group">
          <label>
            <input type="checkbox" name="isSystem" [(ngModel)]="model.isSystem" />
            {{ 'PERMISSIONS.IS_SYSTEM' | translate }}
          </label>
        </div>
        <div class="form-actions">
          <button class="oro-btn oro-btn-primary" type="submit" [disabled]="f.invalid">{{ 'COMMON.SAVE' | translate }}</button>
          <a class="oro-btn oro-btn-ghost" routerLink="/permissions">{{ 'COMMON.CANCEL' | translate }}</a>
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
    .checkbox-group { grid-column: 1 / -1; }
    .checkbox-group label { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; }
    .form-actions { grid-column: 1 / -1; display: flex; gap: 0.75rem; padding-top: 0.5rem; }
  `]
})
export class PermissionsFormComponent implements OnInit {
  private readonly store = inject(PermissionsStore);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isEdit = signal(false);

  model: CreatePermissionCommand = {
    permissionId: '', provider: '', description: '', action: '', resource: '', isSystem: false,
  };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      const perm = this.store.permissions().find(p => p.permissionId === id);
      if (perm) {
        this.model = {
          permissionId: perm.permissionId, provider: perm.provider,
          description: perm.description ?? '', action: perm.action,
          resource: perm.resource, isSystem: perm.isSystem,
        };
      }
    }
  }

  onSubmit(): void {
    if (this.isEdit()) {
      const id = this.route.snapshot.paramMap.get('id')!;
      this.store.update(id, this.model);
      this.toast.show('PERMISSIONS.UPDATED', 'success');
    } else {
      this.store.create({ ...this.model, permissionId: crypto.randomUUID() });
      this.toast.show('PERMISSIONS.CREATED', 'success');
    }
    this.router.navigate(['/permissions']);
  }
}
