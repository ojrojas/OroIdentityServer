import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastService } from '../../shared/toast.service';
import { RolesStore } from './roles.store';

@Component({
  selector: 'app-roles-form',
  standalone: true,
  imports: [FormsModule, RouterLink, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ isEdit() ? ('ROLES.EDIT' | translate) : ('ROLES.NEW' | translate) }}</h2>
      <a class="oro-btn oro-btn-ghost" routerLink="/roles">{{ 'COMMON.BACK' | translate }}</a>
    </div>

    <div class="oro-card form-card">
      <form #f="ngForm" (ngSubmit)="onSubmit()">
        <div class="form-group">
          <label>{{ 'ROLES.NAME' | translate }}</label>
          <input class="oro-input" name="roleName" [(ngModel)]="roleName" required />
        </div>
        <div class="form-actions">
          <button class="oro-btn oro-btn-primary" type="submit" [disabled]="f.invalid">{{ 'COMMON.SAVE' | translate }}</button>
          <a class="oro-btn oro-btn-ghost" routerLink="/roles">{{ 'COMMON.CANCEL' | translate }}</a>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .page-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
    .page-title { font-size: var(--oro-text-2xl); font-weight: 700; flex: 1; }
    .form-card { padding: 1.5rem; max-width: 480px; }
    .form-group { display: flex; flex-direction: column; gap: 0.25rem; margin-bottom: 1rem; }
    .form-group label { font-size: var(--oro-text-sm); font-weight: 500; color: var(--oro-text-secondary); }
    .form-actions { display: flex; gap: 0.75rem; padding-top: 0.5rem; }
  `]
})
export class RolesFormComponent implements OnInit {
  private readonly store = inject(RolesStore);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isEdit = signal(false);
  roleName = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      const role = this.store.roles().find(r => r.id === id);
      if (role) this.roleName = role.name ?? '';
    }
  }

  onSubmit(): void {
    if (this.isEdit()) {
      const id = this.route.snapshot.paramMap.get('id')!;
      this.store.update(id, { roleName: this.roleName });
      this.toast.show('ROLES.UPDATED', 'success');
    } else {
      this.store.create({ roleName: this.roleName });
      this.toast.show('ROLES.CREATED', 'success');
    }
    this.router.navigate(['/roles']);
  }
}
