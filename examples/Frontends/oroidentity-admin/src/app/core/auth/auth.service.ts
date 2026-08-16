import { Injectable, computed, inject, signal } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { UserProfile } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oidc = inject(OidcSecurityService);

  private readonly _isAuthenticated = signal(false);
  private readonly _userProfile = signal<UserProfile | null>(null);
  private readonly _accessToken = signal<string | null>(null);

  readonly isAuthenticated = this._isAuthenticated.asReadonly();
  readonly userProfile = this._userProfile.asReadonly();
  readonly displayName = computed(() => this._userProfile()?.name ?? '');

  constructor() {
    this.oidc.checkAuth().subscribe(({ isAuthenticated, userData, accessToken }) => {
      this._isAuthenticated.set(isAuthenticated);
      this._accessToken.set(accessToken ?? null);
      if (accessToken) {
        localStorage.setItem('access_token', accessToken);
      } else {
        localStorage.removeItem('access_token');
      }

      if (userData) {
        this._userProfile.set({
          sub: userData['sub'],
          email: userData['email'],
          name: userData['name'],
          role: userData['roles'] || [],
          avatarUrl: userData['picture'],
          tenantId: userData['tenant_id'] ?? '',
        });
      }
    });
  }

  login(): void {
    try {

      this.oidc.authorize();
    } catch (error) {
      console.error("login error: ", error);
    }
  }

  logout(): void {
    // Note: do NOT call logoffLocalMultiple() here. logoffAndRevokeTokens() revokes the
    // refresh and access tokens first and reads them from storage *during* the HTTP calls;
    // clearing storage synchronously here makes the second revocation go out without a
    // token, which the server rejects with `missing_token` and aborts the whole logout.
    this.oidc.logoffAndRevokeTokens().subscribe({
      next: () => this.clearLocalSession(),
      error: (error) => {
        console.error('Token revocation failed; falling back to end-session.', error);
        this.clearLocalSession();
        this.oidc.logoff().subscribe();
      },
    });
  }

  private clearLocalSession(): void {
    this._isAuthenticated.set(false);
    this._userProfile.set(null);
    this._accessToken.set(null);
    localStorage.removeItem('access_token');
  }

  getAccessToken(): string | null {
    const token = this._accessToken();
    return token ?? localStorage.getItem('access_token');
  }
}
