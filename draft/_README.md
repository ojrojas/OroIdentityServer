# IdentityServer HTTP Files

Archivos HTTP para probar los endpoints del IdentityServer usando REST Client (VS Code).

## Requisitos

- VS Code con extensión [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

## Configuración

1. Inicia el IdentityServer:
   ```bash
   # Docker
   docker-compose up -d

   # O con Aspire
   aspire run
   ```

2. Abre cualquier archivo `.http` en VS Code

3. Haz clic en "Send Request" sobre cualquier request

## Archivos

| Archivo | Descripción |
|---------|-------------|
| `auth.http` | Login, OAuth2 flows, token refresh |
| `users.http` | CRUD de usuarios |
| `roles.http` | CRUD de roles (AdminOnly) |
| `permissions.http` | CRUD de permisos (AdminOnly) |
| `tenants.http` | CRUD de tenants |
| `applications.http` | CRUD de aplicaciones OpenIddict (MasterAdmin) |
| `scopes.http` | CRUD de scopes (MasterAdmin) |
| `dashboard.http` | Estadísticas del dashboard |
| `sessions.http` | Gestión de sesiones de usuario |
| `validation-logs.http` | Logs de validación |
| `identification-types.http` | Tipos de identificación |

## Credenciales por defecto

| Campo | Valor |
|-------|-------|
| Usuario | `admin` |
| Contraseña | `Admin@123456` |
| Puerto | `5080` |

## Flujo de autenticación recomendado

1. Ejecuta el request de **Login as Admin** en `auth.http`
2. Usa el cookie de respuesta en los demás requests
3. Para OAuth2, sigue el flujo de Authorization Code con PKCE

## Variables

Cada archivo define variables al inicio. Modifícalas según tu entorno:

```
@baseUrl = http://localhost:5080
@adminUser = admin
@adminPass = Admin@123456
```
