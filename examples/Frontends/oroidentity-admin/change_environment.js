const fs = require('fs');

const identityServerHttp = process.env.IDENTITY_API_HTTPS || process.env.IDENTITY_API_HTTP;
const clientId = process.env.CLIENT_ID || ''
const seqUri = process.env.SEQ_URI;

const content = `
export const environment = {
  production: false,
  IDENTITY_SERVER: '${identityServerHttp}',
  CLIENT_ID: '${clientId}',
  SEQ_URI: '${seqUri}'
};
`;

console.log(content);

fs.writeFileSync('./src/environments/environment.development.ts', content);
