import { isDevMode } from "@angular/core";
import { AuthConfig } from "angular-oauth2-oidc";
import { environment } from "../environments/environment";

export const oauthConfig: AuthConfig = {
  issuer: environment.identityServerUrl,
  redirectUri: window.location.origin + '/callback',
  postLogoutRedirectUri: window.location.origin + '/login',
  logoutUrl: environment.identityServerUrl + '/connect/logout',
  clientId: environment.clientId,
  scope: 'openid profile email identity_scope roles',
  responseType: 'code',
  strictDiscoveryDocumentValidation: false,
  showDebugInformation: isDevMode(),
  clearHashAfterLogin: true,
  requestAccessToken: true,
  tokenEndpoint: environment.identityServerUrl + '/connect/token',
  requireHttps: !isDevMode(),
}
