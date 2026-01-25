export const environment = {
  authority: "https://localhost:7219",
  // Client id that will be registered by the server seeder for the SPA
  clientId: "OroIdentityServer.Admin",
  // Angular app will build runtime redirect URIs using window.location.origin
  redirectUriPath: "/signin-oidc",
  postLogoutRedirectPath: "/",
  responseType: "code",
  scope: "openid profile email roles",
};
