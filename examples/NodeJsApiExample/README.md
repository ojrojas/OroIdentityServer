# Node.js API Introspection Example

This example demonstrates how a Node.js/Express API can detect remote logout by introspecting access tokens against the Identity Server.

## How It Works

1. Client sends access token in `Authorization: Bearer <token>` header
2. API calls `POST /connect/introspect` to validate the token
3. If the token is revoked (admin logged the user out), introspection returns `active: false`
4. API returns `401 Unauthorized`
5. Frontend detects 401 and redirects to Identity Server logout

## Setup

1. Register this API in the Identity Server with these permissions:
   - `ept:introspection`
   - `ept:authorization`
   - `ept:token`
   - `gt:client_credentials` (for machine-to-machine) or `gt:authorization_code` (for user-facing)

2. Install dependencies:
   ```bash
   npm install
   ```

3. Set environment variable:
   ```bash
   export IDENTITY_SERVER=https://localhost:5000
   ```

4. Run the API:
   ```bash
   npm start
   ```

## Frontend Integration (React/Angular/Vue)

```javascript
// axios interceptor to handle 401 from revoked tokens
api.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 401) {
            // Token revoked → redirect to Identity Server logout
            window.location.href = `${IDENTITY_SERVER}/connect/logout` +
                `?post_logout_redirect_uri=${encodeURIComponent(window.location.origin)}`;
        }
        return Promise.reject(error);
    }
);
```

## Key Files

- `server.js` - Express API with introspection middleware
- `package.json` - Dependencies
