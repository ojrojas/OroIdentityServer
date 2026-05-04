import {
  ApplicationConfig,
  importProvidersFrom,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';


import { AuthConfig, OAuthModule, OAuthStorage, provideOAuthClient } from 'angular-oauth2-oidc';
import { environment } from '../environments/environment';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { oauthConfig } from './auth.config';

// NGX-Translate imports
import { HttpClient } from '@angular/common/http';
import { errorInterceptor } from './shared/error.interceptor';
import { AuthorizationService } from './core/authorization.service';

export function initAuth(auth: AuthorizationService) {
  return () => auth.runInitialLoginFlow();
}

export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([errorInterceptor])),
    importProvidersFrom(OAuthModule.forRoot({
      resourceServer: {
        allowedUrls: [environment.identityServerUrl],
        sendAccessToken: true,
      }
    }),
      TranslateModule.forRoot({
        loader: {
          provide: TranslateLoader,
          useFactory: HttpLoaderFactory,
          deps: [HttpClient]
        }
      })
    ),
    provideOAuthClient(),
    // Provee la configuración del OAuthService
    { provide: AuthConfig, useValue: oauthConfig },
    // Opcional: Si necesario un almacenamiento personalizado para OAuth (ej. LocalStorage)
    { provide: OAuthStorage, useValue: localStorage },
    AuthService,
    { provide: 'APP_AUTH_INIT', useFactory: initAuth, deps: [AuthService], multi: true }
  ]
