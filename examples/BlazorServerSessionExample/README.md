# Blazor Server Session Example

This example demonstrates how a Blazor Server application can detect remote logout by introspecting access tokens against the Identity Server.

## How It Works

1. **SessionMonitor** - A service that calls `POST /connect/introspect` to validate tokens
2. **SessionValidationMiddleware** - Validates the token on every request
3. If the token is revoked (admin logged the user out remotely), the user is signed out immediately

## Setup

1. Register this app in the Identity Server with these permissions:
   - `ept:introspection`
   - `ept:authorization`
   - `ept:token`
   - `gt:authorization_code`
   - `scp:openid`, `scp:profile`, `scp:email`

2. Configure `appsettings.json`:
   ```json
   {
     "IdentityServer": {
       "Authority": "https://localhost:5000",
       "ClientId": "blazor-server-app",
       "ClientSecret": "your-client-secret"
     }
   }
   ```

3. Run the app:
   ```bash
   dotnet run
   ```

## Flow

1. User logs in via the Identity Server
2. Blazor Server stores the access token in a cookie
3. On every request, `SessionValidationMiddleware` introspects the token
4. If an admin revokes the user's session, the token becomes inactive
5. The middleware detects this and signs the user out locally
6. User is redirected to the login page

## Key Files

- `Services/SessionMonitor.cs` - Calls the introspection endpoint
- `Services/SessionValidationMiddleware.cs` - Validates tokens on each request
- `Program.cs` - Configures the middleware pipeline
