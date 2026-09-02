## Why

El sistema actual de usuarios no soporta jerarquías organizacionales. Los roles (`Administrator`, `Manager`, `User`) son planos y no definen quién reporta a quién. Esto limita la capacidad de:
- Establecer cadenas de mando claras (quién delega tareas a quién)
- Soportar organizaciones matrix donde un usuario reporta a múltiples superiores (funcional + proyecto)
- Gestionar flujos de aprobación basados en la jerarquía real
- Visualizar el organigrama completo del tenant

La necesidad es urgente porque el negocio requiere establecer estructuras organizacionales para delegación de tareas, control de acceso basado en jerarquía, y auditoría de decisiones.

## What Changes

- **NUEVO**: Entity `UserReportingRelationship` que almacena las relaciones de reporte (quién reporta a quién) con tipos (Functional, Project, Matrix, Mentor, Temporary) y prioridad
- **NUEVO**: Entity `RelationshipAuditLog` para historial de cambios de jerarquía
- **NUEVO**: Enum `RelationshipType` con tipos de relación
- **NUEVO**: Servicio `IHierarchyService` con operaciones CRUD de relaciones, consultas jerárquicas recursivas (CTEs PostgreSQL), verificación de autoridad, y árbol organizacional
- **NUEVO**: `HierarchyController` API REST con endpoints para gestión de relaciones, consultas de superiores/subordinados, y árbol jerárquico
- **NUEVO**: Authorization policies (`CanManageHierarchy`, `CanViewSubordinates`, `CanLeadProject`, `CanAssignMatrixRelationships`)
- **NUEVO**: Claims de jerarquía en autenticación (`hierarchy_level`, `direct_superior_ids`, `primary_superior_id`, `relationship_types`)
- **NUEVO**: `HierarchyAuthorizationHandler` para verificación de autoridad en tiempo de ejecución
- **NUEVO**: Componentes Blazor `OrganizationTree` y `RelationshipManager` para visualización y gestión
- **MODIFICADO**: `Role` aggregate - se agrega `Level` (int, 10-100) y `ParentRoleId` para jerarquía de roles
- **MODIFICADO**: `TenantUser` - se renombra `ReportsToUserId` a `PrimaryReportsToUserId` (sincronizado con Functional priority 1)
- **MODIFICADO**: `AdminPasswordSignInService` - se agregan claims de jerarquía al BuildPrincipalAsync
- **MODIFICADO**: `CookieAuthHandlerSetup` - se agregan authorization policies de jerarquía

## Capabilities

### New Capabilities

- `user-hierarchy/relationships`: Gestión de relaciones de reporte entre usuarios (CRUD, tipos, prioridad, validación de ciclos)
- `user-hierarchy/queries`: Consultas jerárquicas recursivas (superiores, subordinados, cadena de mando, árbol organizacional) usando CTEs PostgreSQL
- `user-hierarchy/authorization`: Sistema de autorización basado en jerarquía (policies, claims, verificación de autoridad en tiempo de ejecución)
- `user-hierarchy/api`: API REST para gestión de jerarquía (endpoints CRUD, consultas, árbol)
- `user-hierarchy/ui`: Componentes Blazor para visualización y gestión del organigrama

### Modified Capabilities

- `users/roles`: Se agrega propiedad `Level` (int) y `ParentRoleId` al aggregate Role para definir niveles jerárquicos
- `users/tenants`: Se modifica TenantUser para sincronizar `PrimaryReportsToUserId` con la relación Functional principal
- `authentication/claims`: Se agregan claims de jerarquía al proceso de autenticación

## Impact

- **Backend**: 
  - Nuevas entidades EF Core: `UserReportingRelationship`, `RelationshipAuditLog`
  - Nuevas tablas en PostgreSQL con índices
  - Nuevo servicio `HierarchyService` con CTEs recursivos
  - Nuevo controller `HierarchyController`
  - Modificación en `AdminPasswordSignInService` y `CookieAuthHandlerSetup`
  
- **API**: 
  - Nuevos endpoints REST bajo `/api/hierarchy/*`
  - Nuevas authorization policies
  
- **Frontend**:
  - Nuevos componentes Blazor para organigrama
  - Posible modificación en páginas existentes de usuarios
  
- **Base de Datos**:
  - Migración EF Core con nuevas tablas y columnas
  - Seed data actualizado con roles y niveles jerárquicos
  
- **Dependencias**: No se agregan nuevas dependencias externas

- **Breaking Changes**: 
  - Renombrado de columna `ReportsToUserId` a `PrimaryReportsToUserId` en TenantUser (requiere migración de datos)
  - Código existente que use `TenantUser.ReportsToUserId` debe actualizarse
