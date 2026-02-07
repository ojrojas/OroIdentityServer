import { provideHttpClient } from '@angular/common/http';
import { APP_INITIALIZER, ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withEnabledBlockingInitialNavigation } from '@angular/router';
import { LogLevel, OidcSecurityService, provideAuth } from 'angular-auth-oidc-client';
import { firstValueFrom } from 'rxjs';

import { environment } from '../environments/environment';
import { routes } from './app.routes';

const redirectUri = window.location.origin + environment.redirectUriPath;
const postLogoutRedirectUri = window.location.origin + environment.postLogoutRedirectPath;

export function initAuth(oidc: OidcSecurityService) {
  return () => {
    console.log('[AppInitialiser] Starting OIDC checkAuth...');
    return firstValueFrom(oidc.checkAuth());
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withEnabledBlockingInitialNavigation()),
    provideHttpClient(),
    provideAuth({
      config: {
        authority: environment.authority,
        redirectUrl: redirectUri,
        postLogoutRedirectUri: postLogoutRedirectUri,
        clientId: environment.clientId,
        responseType: environment.responseType,
        scope: environment.scope,
        useRefreshToken: true,
        silentRenew: true,
        autoUserInfo: true,
        renewTimeBeforeTokenExpiresInSeconds: 30,
        logLevel: LogLevel.Debug,

      }
    }),
    {
      provide: APP_INITIALIZER,
      useFactory: initAuth,
      deps: [OidcSecurityService],
      multi: true
    }
  ]
};
