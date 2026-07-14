import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { provideAuth, withAppInitializerAuthCheck } from 'angular-auth-oidc-client';
import { provideMarkdown } from 'ngx-markdown';
import { environment } from '../environments/environment';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { acceptLanguageInterceptor } from './core/services/accept-language.interceptor';
import { companyInterceptor } from './core/services/company.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([authInterceptor, companyInterceptor, acceptLanguageInterceptor])),
    provideMarkdown(),
    provideTranslateService({ fallbackLang: 'es' }),
    provideTranslateHttpLoader({ prefix: './assets/i18n/', suffix: '.json' }),
    provideAuth({
      config: {
        authority: environment.IDENTITY_SERVER,
        redirectUrl: window.location.origin + '/auth/callback',
        postLogoutRedirectUri: window.location.origin,
        clientId: environment.CLIENT_ID,
        scope: 'openid profile email roles offline_access',
        responseType: 'code',
        silentRenew: true,
        useRefreshToken: true,
        ignoreNonceAfterRefresh: true,
        triggerRefreshWhenIdTokenExpired: false,
      }
    }, withAppInitializerAuthCheck()),
  ]
};
