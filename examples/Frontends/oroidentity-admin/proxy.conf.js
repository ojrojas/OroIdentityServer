// Dev-server proxy for the Angular example.
//
// The SPA calls its API with relative "/api/..." URLs (see the *Store files). Without this rule the
// Angular dev server answers them itself: it returns index.html with 200 text/html, so every
// request "succeeds" and then fails to parse as JSON, and the API is never reached at all.
//
// The OIDC endpoints are NOT proxied: the browser must talk to the identity server on its own
// origin (the authority comes from environment.IDENTITY_SERVER) or the secure SameSite=Lax cookie
// set by /auth/login is not sent on the redirect back from /connect/authorize.

const identityServer =
  process.env.IDENTITY_API_HTTPS || process.env.IDENTITY_API_HTTP;

module.exports = {
  "/api": {
    target: identityServer,
    secure: process.env["NODE_ENV"] !== "development",
    changeOrigin: true,
    logLevel: "debug",
  },

  "/connect": {
    target: identityServer,
    secure: process.env["NODE_ENV"] !== "development",
    changeOrigin: true,
    logLevel: "debug",
  },

  "/.well-known": {
    target: identityServer,
    secure: process.env["NODE_ENV"] !== "development",
    changeOrigin: true,
    logLevel: "debug",
  },
};

console.debug(module.exports);