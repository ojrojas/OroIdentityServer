import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Observable, map } from 'rxjs';

@Component({
  standalone: true,
  selector: 'app-home',
  imports: [CommonModule],
  template: `
    <div style="padding:1rem">
      <h2>Identity Admin</h2>

      <div *ngIf="isAuth$ | async; else loadingOrRedirecting">
        <div style="background: #e8f5e9; border: 1px solid #4caf50; padding: 1rem; border-radius: 4px; margin-bottom: 1rem;">
           <strong>Estado:</strong> Is Authenticated ✅
        </div>

        <button (click)="logout()" style="padding: 0.5rem 1rem; background: #f44336; color: white; border: none; border-radius: 4px; cursor: pointer;">
          Logout session
        </button>

        <div style="margin-top: 2rem">
          <h3>User Information</h3>
          <pre style="background: #f5f5f5; padding: 1rem; overflow: auto;">{{ userData$ | async | json }}</pre>

          <h3>Tokens (Debug)</h3>
          <p><strong>Access Token:</strong> <code style="word-break: break-all;">{{ accessToken$ | async }}</code></p>
          <p><strong>ID Token:</strong> <code style="word-break: break-all;">{{ idToken$ | async }}</code></p>
        </div>
      </div>

      <ng-template #loadingOrRedirecting>
        <div style="padding: 2rem; text-align: center;">
          <p>Checking authentication...</p>
          <p style="font-size: 0.8rem; color: #666">If you are not logged in, you will be redirected automatically.</p>
          <button (click)="login()" style="margin-top: 1rem">Force Redirect</button>
        </div>
      </ng-template>
    </div>
  `
})
export class HomeComponent implements OnInit {
  constructor(private oidc: OidcSecurityService) {}

  get isAuth$(): Observable<boolean> {
    return this.oidc.isAuthenticated$.pipe(map(result => result.isAuthenticated));
  }

  get userData$(): Observable<any> {
    return this.oidc.userData$;
  }

  get accessToken$(): Observable<string> {
    return this.oidc.getAccessToken();
  }

  get idToken$(): Observable<string> {
    return this.oidc.getIdToken();
  }

  ngOnInit(): void {
    this.oidc.isAuthenticated$.subscribe(result => {
      console.log('[HomeComponent] isAuth:', result.isAuthenticated);
    });
  }

  login() {
    this.oidc.authorize();
  }

  logout() {
    this.oidc.logoff();
  }
}
