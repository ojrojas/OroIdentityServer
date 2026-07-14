import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastService } from '../../shared/toast.service';
import { ScopesStore } from './scopes.store';

@Component({
  selector: 'app-scopes-form',
  standalone: true,
  imports: [FormsModule, RouterLink, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ isEdit() ? ('SCOPES.EDIT' | translate) : ('SCOPES.NEW' | translate) }}</h2>
      <a class="oro-btn oro-btn-ghost" routerLink="/scopes">{{ 'COMMON.BACK' | translate }}</a>
    </div>

    <div class="oro-card form-card">
      <form #f="ngForm" (ngSubmit)="onSubmit()" class="form-grid">
        <div class="form-group">
          <label>{{ 'SCOPES.NAME' | translate }}</label>
          <input class="oro-input" name="name" [(ngModel)]="name" required
                 [attr.readonly]="isEdit() ? true : null" />
        </div>
        <div class="form-group full-width">
          <label>{{ 'SCOPES.RESOURCES' | translate }}</label>
          <textarea class="oro-input" name="resources" [(ngModel)]="resourcesStr" rows="3"
                    placeholder="One resource per line"></textarea>
        </div>
        <div class="form-actions">
          <button class="oro-btn oro-btn-primary" type="submit" [disabled]="f.invalid">{{ 'COMMON.SAVE' | translate }}</button>
          <a class="oro-btn oro-btn-ghost" routerLink="/scopes">{{ 'COMMON.CANCEL' | translate }}</a>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .page-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
    .page-title { font-size: var(--oro-text-2xl); font-weight: 700; flex: 1; }
    .form-card { padding: 1.5rem; max-width: 480px; }
    .form-grid { display: grid; grid-template-columns: 1fr; gap: 1rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.25rem; }
    .form-group label { font-size: var(--oro-text-sm); font-weight: 500; color: var(--oro-text-secondary); }
    .full-width { grid-column: 1 / -1; }
    .form-actions { display: flex; gap: 0.75rem; padding-top: 0.5rem; }
    textarea.oro-input { resize: vertical; min-height: 3rem; }
  `]
})
export class ScopesFormComponent implements OnInit {
  private readonly store = inject(ScopesStore);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isEdit = signal(false);
  name = '';
  resourcesStr = '';

  ngOnInit(): void {
    const scopeName = this.route.snapshot.paramMap.get('name');
    if (scopeName) {
      this.isEdit.set(true);
      this.name = scopeName;
      const scope = this.store.scopes().find(s => s.name === scopeName);
      if (scope) this.resourcesStr = (scope.resources ?? []).join('\n');
    }
  }

  onSubmit(): void {
    const resources = this.resourcesStr.split('\n').filter(Boolean);
    if (this.isEdit()) {
      this.store.update(this.name, { name: this.name, resources });
      this.toast.show('SCOPES.UPDATED', 'success');
    } else {
      this.store.create({ name: this.name, resources });
      this.toast.show('SCOPES.CREATED', 'success');
    }
    this.router.navigate(['/scopes']);
  }
}
