# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# Build stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Build.targets Directory.Packages.props ./
COPY src/ src/

RUN dotnet restore src/IdentityServer/IdentityServer/IdentityServer.csproj
RUN dotnet publish src/IdentityServer/IdentityServer/IdentityServer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-self-contained

# ---------------------------------------------------------------------------
# Runtime stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER $APP_UID

COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

# ---------------------------------------------------------------------------
# Configuration (every value below can be overridden at run time with -e, or
# in a compose/kubernetes file; the connection string and signing key MUST be
# changed for real deployments).
# ---------------------------------------------------------------------------

# ASP.NET Core host. The image always starts HTTP-only (port 5080), so it runs without needing
# a certificate. To also serve HTTPS, override ASPNETCORE_URLS at run time
# (e.g. "http://+:5080;https://+:5086") and point Kestrel at a mounted certificate via
# Kestrel__Certificates__Default__Path (+ __Password for PFX, or __KeyPath for a PEM key).
# A https URL without a certificate fails at startup, so keep the default below HTTP-only.
ENV ASPNETCORE_URLS=http://+:5080;https://+:5086
ENV ASPNETCORE_ENVIRONMENT=Production

# PostgreSQL. Uses the host/container name `db` (see docker-compose.yaml).
ENV ConnectionStrings__identitydb="Host=db;Port=5432;Database=identitydb;Username=postgres;Password=Weak(!)Password123"

# OpenIddict signing/encryption key (base64, >= 32 bytes). Shared with all
# instances issuing/validating tokens. Override with a random value:
#   openssl rand -base64 32
ENV SymmetricSecurityKey="aXRlbnRpb25hbF9wcm9wb3NlX21pZ3JhdGlvbnM="

# Base URL of the external admin SPA. Used only when the OpenIddict "Admin"
# client is seeded, so it can stay a placeholder if that client is not used.
ENV IDENTITY_ADMIN_HTTP="http://localhost:4200"

# Seeder control. Set to true to skip seeding entirely.
ENV DatabaseSeeder__Skip="false"

# Tenant created during seeding
ENV SEED_TENANT_NAME="OroMasterRealm"

# Universal bootstrap admin account. It is granted the Administrator role and
# is exempt from the forced first-login password change unless
# SEED_ADMIN_FORCE_PASSWORD_CHANGE=true.
ENV SEED_ADMIN_USERNAME="admin"
ENV SEED_ADMIN_PASSWORD="Admin@123456"
ENV SEED_ADMIN_EMAIL="admin@example.com"
ENV SEED_ADMIN_NAME="Admin"
ENV SEED_ADMIN_LASTNAME="Administrator"
ENV SEED_ADMIN_IDENTIFICATION="000000001"
ENV SEED_ADMIN_ROLE="Administrator"
ENV SEED_ADMIN_FORCE_PASSWORD_CHANGE="false"

# RabbitMQ event bus (optional; only needed for event-driven features).
ENV EventBus__RabbitMQ__HostName="localhost"
ENV EventBus__RabbitMQ__Port="5672"
ENV EventBus__RabbitMQ__UserName="guest"
ENV EventBus__RabbitMQ__Password="guest"
ENV EventBus__RabbitMQ__VirtualHost="/"

ENV Branding__AppName="OroIdentity"
ENV Branding__FullName="OroIdentityServer"
ENV Branding__DisplayName="OroIdentity Admin SPA"
ENV Branding__LogoUrl=
ENV Branding__FullLogoUrl=
ENV Branding__EffectiveLogoUrl=
ENV Branding__EffectiveFullLogoUrl=

EXPOSE 5080
EXPOSE 5086

ENTRYPOINT ["dotnet", "IdentityServer.dll"]
