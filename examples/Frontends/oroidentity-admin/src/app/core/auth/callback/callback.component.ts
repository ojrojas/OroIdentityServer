import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-callback',
  standalone: true,
  template: `
    <div style="height:100vh;display:flex;align-items:center;justify-content:center;flex-direction:column;gap:1rem;">
      <div class="oro-spinner"></div>
      <p class="text-muted text-sm">Procesando autenticación...</p>
    </div>
  `
})
export class CallbackComponent implements OnInit {
  private readonly oidc = inject(OidcSecurityService);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);

  ngOnInit(): void {
    this.oidc.checkAuth().subscribe(({ isAuthenticated }) => {
      if (isAuthenticated) {
        this.router.navigate(['/dashboard']);
      } else {
        this.auth.login();
      }
    });
  }
}
