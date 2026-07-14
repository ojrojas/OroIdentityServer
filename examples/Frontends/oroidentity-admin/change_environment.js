const fs = require('fs');

const identityServerHttp = process.env.IDENTITY_SERVER_HTTP ||process.env.IDENTITY_SERVER_HTTPS || 'https://localhost:7219' ;
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
