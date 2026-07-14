import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastService } from '../../shared/toast.service';
import { ApplicationsStore } from './applications.store';
import { ApplicationDto } from '../../core/models';

@Component({
  selector: 'app-applications-form',
  standalone: true,
  imports: [FormsModule, RouterLink, TranslatePipe],
  template: `
    <div class="page-header">
      <h2 class="page-title">{{ isEdit() ? ('APPLICATIONS.EDIT' | translate) : ('APPLICATIONS.NEW' | translate) }}</h2>
      <a class="oro-btn oro-btn-ghost" routerLink="/applications">{{ 'COMMON.BACK' | translate }}</a>
    </div>

    <div class="oro-card form-card">
      <form #f="ngForm" (ngSubmit)="onSubmit()" class="form-grid">
        <div class="form-group">
          <label>{{ 'APPLICATIONS.CLIENT_ID' | translate }}</label>
          <input class="oro-input" name="clientId" [(ngModel)]="model.clientId" required
                 [attr.readonly]="isEdit() ? true : null" />
        </div>
        <div class="form-group">
          <label>{{ 'APPLICATIONS.DISPLAY_NAME' | translate }}</label>
          <input class="oro-input" name="displayName" [(ngModel)]="model.displayName" />
        </div>
        <div class="form-group">
          <label>{{ 'APPLICATIONS.CLIENT_TYPE' | translate }}</label>
          <input class="oro-input" name="clientType" [(ngModel)]="model.clientType" />
        </div>
        <div class="form-group">
          <label>{{ 'APPLICATIONS.APPLICATION_TYPE' | translate }}</label>
          <input class="oro-input" name="applicationType" [(ngModel)]="model.applicationType" />
        </div>
        @if (!isEdit()) {
          <div class="form-group">
            <label>{{ 'APPLICATIONS.CLIENT_SECRET' | translate }}</label>
            <input class="oro-input" name="clientSecret" type="password" [(ngModel)]="model.clientSecret" />
          </div>
        }
        <div class="form-group">
          <label>{{ 'APPLICATIONS.CONSENT_TYPE' | translate }}</label>
          <input class="oro-input" name="consentType" [(ngModel)]="model.consentType" />
        </div>
        <div class="form-group full-width">
          <label>{{ 'APPLICATIONS.REDIRECT_URIS' | translate }}</label>
          <textarea class="oro-input" name="redirectUris" [(ngModel)]="redirectUris" rows="2"
                    placeholder="One per line"></textarea>
        </div>
        <div class="form-group full-width">
          <label>{{ 'APPLICATIONS.POST_LOGOUT_URIS' | translate }}</label>
          <textarea class="oro-input" name="postLogoutUris" [(ngModel)]="postLogoutUris" rows="2"
                    placeholder="One per line"></textarea>
        </div>
        <div class="form-actions">
          <button class="oro-btn oro-btn-primary" type="submit" [disabled]="f.invalid">{{ 'COMMON.SAVE' | translate }}</button>
          <a class="oro-btn oro-btn-ghost" routerLink="/applications">{{ 'COMMON.CANCEL' | translate }}</a>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .page-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
    .page-title { font-size: var(--oro-text-2xl); font-weight: 700; flex: 1; }
    .form-card { padding: 1.5rem; max-width: 720px; }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.25rem; }
    .form-group label { font-size: var(--oro-text-sm); font-weight: 500; color: var(--oro-text-secondary); }
    .full-width { grid-column: 1 / -1; }
    .form-actions { grid-column: 1 / -1; display: flex; gap: 0.75rem; padding-top: 0.5rem; }
    textarea.oro-input { resize: vertical; min-height: 3rem; font-family: var(--oro-font-mono, monospace); font-size: 0.85em; }
  `]
})
export class ApplicationsFormComponent implements OnInit {
  private readonly store = inject(ApplicationsStore);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isEdit = signal(false);
  redirectUris = '';
  postLogoutUris = '';

  model: ApplicationDto = {
    clientId: '', displayName: '', clientType: '', applicationType: '',
    clientSecret: '', consentType: '', permissions: [], requirements: [],
    redirectUris: [], postLogoutRedirectUris: [],
  };

  ngOnInit(): void {
    const clientId = this.route.snapshot.paramMap.get('clientId');
    if (clientId) {
      this.isEdit.set(true);
      const app = this.store.applications().find(a => a.clientId === clientId);
      if (app) {
        this.model = { ...app };
        this.redirectUris = (app.redirectUris ?? []).join('\n');
        this.postLogoutUris = (app.postLogoutRedirectUris ?? []).join('\n');
      }
    }
  }

  onSubmit(): void {
    const payload: ApplicationDto = {
      ...this.model,
      redirectUris: this.redirectUris.split('\n').filter(Boolean),
      postLogoutRedirectUris: this.postLogoutUris.split('\n').filter(Boolean),
    };

    if (this.isEdit()) {
      this.store.update(this.model.clientId, payload);
      this.toast.show('APPLICATIONS.UPDATED', 'success');
    } else {
      this.store.create(payload);
      this.toast.show('APPLICATIONS.CREATED', 'success');
    }
    this.router.navigate(['/applications']);
  }
}
