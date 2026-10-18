// Generates the Angular environment file for one dev-server instance.
//
// This used to write a single hardcoded path, which breaks as soon as two instances run at the same
// time: the implicit example and the explicit example would both rewrite
// environment.development.ts and each could end up asking for the other's client id. ENV_TARGET lets
// every instance own its own file, and angular.json maps each target to a build configuration.

const fs = require('fs');
const path = require('path');

const identityServerHttp = process.env.IDENTITY_API_HTTPS || process.env.IDENTITY_API_HTTP;
const clientId = process.env.CLIENT_ID || '';
const seqUri = process.env.SEQ_URI;
const consentType = process.env.CONSENT_TYPE || 'implicit';

// Kept as the default so an existing `pnpm start` behaves exactly as before.
const target = process.env.ENV_TARGET || 'environment.development.ts';
const targetPath = path.join(__dirname, 'src', 'environments', path.basename(target));

if (!targetPath.startsWith(path.join(__dirname, 'src', 'environments'))) {
  throw new Error(`ENV_TARGET must stay inside src/environments, got: ${target}`);
}

const content = `
export const environment = {
  production: false,
  IDENTITY_SERVER: '${identityServerHttp}',
  CLIENT_ID: '${clientId}',
  CONSENT_TYPE: '${consentType}',
  SEQ_URI: '${seqUri}'
};
`;

console.log(`[change_environment] ${path.basename(targetPath)} -> clientId='${clientId}' consent='${consentType}' authority='${identityServerHttp}'`);

fs.writeFileSync(targetPath, content);
