# Plan: Sistema Jerárquico tipo Cadena de Mando

## Visión General

Implementar un sistema jerárquico híbrido que combine:
1. **Roles con niveles** - Definen permisos y autoridad
2. **Asignación de superior(es) por usuario** - Cadena de mando real por tenant
3. **Soporte Matrix** - Usuarios pueden tener múltiples superiores con tipos

## Casos Soportados

### Caso 1: Un Solo Jefe (Árbol Simple)
```
Manager A
    ├── Coordinator B
    │       └── Operator C
    └── Coordinator D
```
Cada usuario tiene UN superior directo. Cadena clara y lineal.

### Caso 2: Varios Jefes (Organización Matrix)
```
Manager A (Funcional)          Manager B (Proyecto X)
    │                               │
    └──────────┬────────────────────┘
               │
        Operator C
```
El Operator C reporta a AMBOS managers (funcional + proyecto).

## Arquitectura Propuesta

### 1. Modelo de Datos

#### 1.1 Extender `Role` Aggregate
```csharp
// Role.cs - Agregar propiedades
public class Role : AggregateRoot<RoleId>
{
    // ... existente ...
    public string Name { get; private set; }
    
    // NUEVO: Nivel jerárquico (mayor = más autoridad)
    public int Level { get; private set; }
    
    // NUEVO: Rol padre para jerarquía de roles
    public RoleId? ParentRoleId { get; private set; }
}
```

**Niveles sugeridos:**
- 100: Master Admin
- 90: Admin
- 80: Director
- 70: Manager
- 60: Coordinator
- 50: Supervisor
- 40: Operator
- 30: Worker
- 20: Client
- 10: Guest

#### 1.2 Nueva Entity: `UserReportingRelationship`
```csharp
// Relación de reporte (puede haber múltiples por usuario)
public class UserReportingRelationship : Entity<UserReportingRelationshipId>
{
    public UserReportingRelationshipId Id { get; private set; }
    public UserId UserId { get; private set; }        // Quién reporta
    public UserId ReportsToUserId { get; private set; } // A quién reporta
    public TenantId TenantId { get; private set; }    // Scoped por tenant
    public RelationshipType Type { get; private set; } // Funcional, Proyecto, etc.
    public int Priority { get; private set; }         // 1=Principal, 2+,3...=Secundario
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    
    // Navigation
    public User User { get; private set; }
    public User ReportsToUser { get; private set; }
    public Tenant Tenant { get; private set; }
}

// Tipos de relación
public enum RelationshipType
{
    Functional = 1,    // Relación funcional/jerárquica directa
    Project = 2,       // Líder de proyecto
    Matrix = 3,        // Relación matricial
    Mentor = 4,        // Mentoría (sin autoridad directa)
    Temporary = 5      // Temporal (ej: cobertura de ausencia)
}
```

**Ventajas sobre ReportsToUserId en TenantUser:**
- Un usuario puede reportar a MÚLTIPLES superiores
- Cada relación tiene un TIPO (funcional, proyecto, etc.)
- Cada relación tiene PRIORIDAD (principal vs secundario)
- Fácil agregar nuevos tipos de relación
- Histórico de cambios por separate entity

#### 1.3 Extender `TenantUser` (mantener para backward compatibility)
```csharp
// TenantUser.cs - Mantener para queries simples
public class TenantUser : Entity<TenantUserId>
{
    // ... existente ...
    public TenantId TenantId { get; private set; }
    public UserId UserId { get; private set; }
    public bool IsActive { get; private set; }
    
    // MANTENER: Superior principal (sincrónico con UserReportingRelationship principal)
    // Se usa para queries rápidas y backward compatibility
    public UserId? PrimaryReportsToUserId { get; private set; }
    
    // MANTENER: Nivel jerárquico efectivo
    public int HierarchyLevel { get; private set; }
}
```

**Nota:** `PrimaryReportsToUserId` se sincroniza automáticamente con la relación de tipo `Functional` con prioridad 1.

#### 1.4 Entity: `RelationshipAuditLog` (Auditoría)
```csharp
// Para historial de cambios de relaciones
public class RelationshipAuditLog : Entity<RelationshipAuditLogId>
{
    public UserId UserId { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId ReportsToUserId { get; private set; }
    public RelationshipType Type { get; private set; }
    public string Action { get; private set; } // Created, Updated, Deleted
    public DateTime ChangedAtUtc { get; private set; }
    public UserId ChangedByUserId { get; private set; }
    public string? Reason { get; private set; }
}
```

### 2. Servicios de Jerarquía

#### 2.1 `IHierarchyService`
```csharp
public interface IHierarchyService
{
    // ==========================================
    // GESTIÓN DE RELACIONES
    // ==========================================
    
    // Crear relación de reporte
    Task<UserReportingRelationship> CreateRelationshipAsync(
        UserId userId, 
        UserId reportsToUserId, 
        TenantId tenantId,
        RelationshipType type,
        int priority = 1);
    
    // Actualizar prioridad de relación
    Task UpdateRelationshipPriorityAsync(
        UserReportingRelationshipId relationshipId,
        int newPriority);
    
    // Eliminar relación (soft delete)
    Task DeleteRelationshipAsync(UserReportingRelationshipId relationshipId);
    
    // Obtener todas las relaciones de un usuario
    Task<IReadOnlyCollection<UserReportingRelationshipDto>> GetUserRelationshipsAsync(
        UserId userId, TenantId tenantId);
    
    // ==========================================
    // CONSULTAS DE JERARQUÍA
    // ==========================================
    
    // Obtener superiores directos (pueden ser múltiples)
    Task<IReadOnlyCollection<UserSuperiorDto>> GetDirectSuperiorsAsync(
        UserId userId, TenantId tenantId);
    
    // Obtener superior principal (el de Functional con prioridad 1)
    Task<UserSuperiorDto?> GetPrimarySuperiorAsync(
        UserId userId, TenantId tenantId);
    
    // Obtener superiores por tipo
    Task<IReadOnlyCollection<UserSuperiorDto>> GetSuperiorsByTypeAsync(
        UserId userId, TenantId tenantId, RelationshipType type);
    
    // Obtener subordinados directos (pueden reportar por diferentes tipos)
    Task<IReadOnlyCollection<UserSubordinateDto>> GetDirectSubordinatesAsync(
        UserId userId, TenantId tenantId);
    
    // Obtener TODOS los subordinados (recursivo - sigue la jerarquía principal)
    Task<IReadOnlyCollection<UserSubordinateDto>> GetAllSubordinatesAsync(
        UserId userId, TenantId tenantId);
    
    // Obtener cadena de mando completa (hasta la raíz, sigue prioridad principal)
    Task<IReadOnlyCollection<UserSuperiorDto>> GetCommandChainAsync(
        UserId userId, TenantId tenantId);
    
    // ==========================================
    // VERIFICACIÓN DE AUTORIDAD
    // ==========================================
    
    // Verificar si usuario A puede comandar a usuario B
    // (Ambos superiores tienen autoridad total)
    Task<bool> CanCommandAsync(
        UserId commanderUserId, UserId targetUserId, TenantId tenantId);
    
    // Verificar por tipo específico
    Task<bool> CanCommandByTypeAsync(
        UserId commanderUserId, UserId targetUserId, TenantId tenantId,
        RelationshipType type);
    
    // ==========================================
    // ÁRBOL JERÁRQUICO
    // ==========================================
    
    // Obtener árbol jerárquico completo del tenant
    // (Usa la relación principal para la estructura del árbol)
    Task<HierarchyTreeDto> GetOrganizationTreeAsync(TenantId tenantId);
    
    // Obtener árbol visualizando todas las relaciones
    Task<HierarchyTreeDto> GetFullOrganizationTreeAsync(TenantId tenantId);
    
    // ==========================================
    // UTILIDADES
    // ==========================================
    
    // Obtener nivel jerárquico
    Task<int> GetHierarchyLevelAsync(UserId userId, TenantId tenantId);
    
    // Sincronizar PrimaryReportsToUserId en TenantUser
    Task SyncPrimaryReportsToAsync(UserId userId, TenantId tenantId);
}
```

#### 2.2 `HierarchyService` Implementation
```csharp
public class HierarchyService : IHierarchyService
{
    private readonly OroIdentityAppContext _context;
    
    // ==========================================
    // GESTIÓN DE RELACIONES
    // ==========================================
    
    public async Task<UserReportingRelationship> CreateRelationshipAsync(
        UserId userId, UserId reportsToUserId, TenantId tenantId,
        RelationshipType type, int priority = 1)
    {
        // 1. Validar que no exista ya esta relación
        // 2. Validar que no se cree un ciclo
        // 3. Crear la relación
        // 4. Si es Functional con prioridad 1, sincronizar TenantUser.PrimaryReportsToUserId
        // 5. Registrar en auditoría
    }
    
    // ==========================================
    // CONSULTAS (CTEs Recursivos)
    // ==========================================
    
    // Para múltiples superiores, los CTEs necesitan modificar:
    // - UNION en lugar de UNION ALL cuando hay múltiples paths
    // - Usar visited[] para evitar ciclos
    // - Merge de resultados de múltiples paths
    
    public async Task<IReadOnlyCollection<UserSuperiorDto>> GetDirectSuperiorsAsync(
        UserId userId, TenantId tenantId)
    {
        // Query simple: JOIN UserReportingRelationship WHERE UserId = @userId
    }
    
    public async Task<IReadOnlyCollection<UserSubordinateDto>> GetAllSubordinatesAsync(
        UserId userId, TenantId tenantId)
    {
        // CTE Recursivo que sigue la relación PRINCIPAL (Functional prioridad 1)
        // Para ver TODOS los subordinados (incluso los de tipo Project),
        // usar GetFullOrganizationTreeAsync
    }
}
```

#### 2.3 Consultas SQL Actualizadas (PostgreSQL)

**CTE para múltiples superiores:**
```sql
-- Obtener todos los superiores de un usuario (recursivo, múltiples paths)
WITH RECURSIVE superiors AS (
    -- Base: superiores directos
    SELECT 
        urr.ReportsToUserId,
        urr.Type,
        urr.Priority,
        u.UserName,
        u.Name || ' ' || u.LastName as FullName,
        1 as Depth
    FROM UserReportingRelationship urr
    JOIN "User" u ON urr.ReportsToUserId = u.Id
    WHERE urr.UserId = @UserId 
      AND urr.TenantId = @TenantId
      AND urr.IsActive = true
    
    UNION ALL
    
    -- Recursivo: superiores de superiores
    SELECT 
        urr2.ReportsToUserId,
        urr2.Type,
        urr2.Priority,
        u2.UserName,
        u2.Name || ' ' || u2.LastName,
        s.Depth + 1
    FROM UserReportingRelationship urr2
    JOIN "User" u2 ON urr2.ReportsToUserId = u2.Id
    JOIN superiors s ON urr2.UserId = s.ReportsToUserId
    WHERE urr2.TenantId = @TenantId
      AND urr2.IsActive = true
      AND s.Depth < 10  -- Límite de profundidad
)
SELECT DISTINCT ON (ReportsToUserId) * 
FROM superiors 
ORDER BY ReportsToUserId, Priority ASC;
```

**CTE para subordinados (árboles múltiples):**
```sql
-- Obtener todos los subordinados (considerando múltiples jefes)
WITH RECURSIVE subordinates AS (
    -- Base: subordinados directos (todas las relaciones)
    SELECT 
        urr.UserId,
        urr.Type,
        urr.Priority,
        u.UserName,
        u.Name || ' ' || u.LastName as FullName,
        ARRAY[urr.UserId] as Visited,  -- Para evitar ciclos
        1 as Depth
    FROM UserReportingRelationship urr
    JOIN "User" u ON urr.UserId = u.Id
    WHERE urr.ReportsToUserId = @UserId 
      AND urr.TenantId = @TenantId
      AND urr.IsActive = true
    
    UNION ALL
    
    -- Recursivo: subordinados de subordinados
    SELECT 
        urr2.UserId,
        urr2.Type,
        urr2.Priority,
        u2.UserName,
        u2.Name || ' ' || u2.LastName,
        s.Visited || urr2.UserId,
        s.Depth + 1
    FROM UserReportingRelationship urr2
    JOIN "User" u2 ON urr2.UserId = u2.Id
    JOIN subordinates s ON urr2.ReportsToUserId = s.UserId
    WHERE urr2.TenantId = @TenantId
      AND urr2.IsActive = true
      AND NOT urr2.UserId = ANY(s.Visited)  -- Evitar ciclos
      AND s.Depth < 10
)
SELECT DISTINCT ON (UserId) * 
FROM subordinates 
ORDER BY UserId, Priority ASC;
```

### 3. Claims & Authorization

#### 3.1 Nuevos Claims
```csharp
// En AdminPasswordSignInService.BuildPrincipalAsync()
// Agregar claims de jerarquía

// Nivel jerárquico (del rol)
claims.Add(new Claim("hierarchy_level", hierarchyLevel.ToString()));

// Superiores directos (JSON array de IDs)
var superiors = await _hierarchyService.GetDirectSuperiorsAsync(userId, tenantId);
var superiorIds = superiors.Select(s => s.UserId.Value).ToList();
claims.Add(new Claim("direct_superior_ids", JsonSerializer.Serialize(superiorIds)));

// Superior principal (Functional prioridad 1)
var primarySuperior = await _hierarchyService.GetPrimarySuperiorAsync(userId, tenantId);
claims.Add(new Claim("primary_superior_id", primarySuperior?.UserId.Value.ToString() ?? ""));

// Tipos de relación del usuario
var relationships = await _hierarchyService.GetUserRelationshipsAsync(userId, tenantId);
var relationshipTypes = relationships.Select(r => r.Type.ToString()).ToList();
claims.Add(new Claim("relationship_types", JsonSerializer.Serialize(relationshipTypes)));
```

#### 3.2 Nuevas Authorization Policies
```csharp
// En CookieAuthHandlerSetup.cs

// Policy: Puede gestionar jerarquía (Manager o superior)
options.AddPolicy("CanManageHierarchy", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(c => 
            c.Type == "hierarchy_level" && 
            int.Parse(c.Value) >= 70)));

// Policy: Puede ver subordinados (Operator o superior)
options.AddPolicy("CanViewSubordinates", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(c => 
            c.Type == "hierarchy_level" && 
            int.Parse(c.Value) >= 40)));

// Policy: Puede ser líder de proyecto (Coordinator o superior)
options.AddPolicy("CanLeadProject", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(c => 
            c.Type == "hierarchy_level" && 
            int.Parse(c.Value) >= 60)));

// Policy: Puede asignar relaciones matrix
options.AddPolicy("CanAssignMatrixRelationships", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(c => 
            c.Type == "hierarchy_level" && 
            int.Parse(c.Value) >= 60))); // Coordinator o superior
```

#### 3.3 Verificación de Autoridad en Tiempo de Ejecución
```csharp
// Para verificación en tiempo de ejecución (no solo claims)
public class HierarchyAuthorizationHandler : AuthorizationHandler<HierarchyRequirement>
{
    private readonly IHierarchyService _hierarchyService;
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        HierarchyRequirement requirement)
    {
        var userId = GetUserId(context.User);
        var targetUserId = requirement.TargetUserId;
        var tenantId = GetTenantId(context);
        
        // Verificar si puede comandar al usuario objetivo
        bool canCommand = requirement.RelationshipType.HasValue
            ? await _hierarchyService.CanCommandByTypeAsync(
                userId, targetUserId, tenantId, requirement.RelationshipType.Value)
            : await _hierarchyService.CanCommandAsync(userId, targetUserId, tenantId);
        
        if (canCommand)
            context.Succeed(requirement);
    }
}

// Uso en controllers
[Authorize(Policy = "CanManageHierarchy")]
[Authorize(HierarchyRequirement = new { TargetUserId = id })]
public async Task<IActionResult> EditUser(Guid id) { ... }
```

### 4. API Endpoints

#### 4.1 `HierarchyController`
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HierarchyController : ControllerBase
{
    private readonly IHierarchyService _hierarchyService;
    
    // ==========================================
    // GESTIÓN DE RELACIONES
    // ==========================================
    
    // POST /api/hierarchy/relationships
    [HttpPost("relationships")]
    [Authorize(Policy = "CanManageHierarchy")]
    public async Task<ActionResult<UserReportingRelationshipDto>> CreateRelationship(
        [FromBody] CreateRelationshipRequest request)
    
    // PUT /api/hierarchy/relationships/{id}/priority
    [HttpPut("relationships/{id}/priority")]
    [Authorize(Policy = "CanManageHierarchy")]
    public async Task<IActionResult> UpdateRelationshipPriority(
        Guid id, [FromBody] UpdatePriorityRequest request)
    
    // DELETE /api/hierarchy/relationships/{id}
    [HttpDelete("relationships/{id}")]
    [Authorize(Policy = "CanManageHierarchy")]
    public async Task<IActionResult> DeleteRelationship(Guid id)
    
    // GET /api/hierarchy/relationships/{userId}
    [HttpGet("relationships/{userId}")]
    public async Task<ActionResult<IReadOnlyCollection<UserReportingRelationshipDto>>> 
        GetUserRelationships(Guid userId)
    
    // ==========================================
    // CONSULTAS DE JERARQUÍA
    // ==========================================
    
    // GET /api/hierarchy/superiors/{userId?}
    [HttpGet("superiors/{userId?}")]
    public async Task<ActionResult<IReadOnlyCollection<UserSuperiorDto>>> 
        GetDirectSuperiors(Guid? userId)
    
    // GET /api/hierarchy/superiors/{userId?}/primary
    [HttpGet("superiors/{userId?}/primary")]
    public async Task<ActionResult<UserSuperiorDto?>> GetPrimarySuperior(Guid? userId)
    
    // GET /api/hierarchy/superiors/{userId?}/by-type/{type}
    [HttpGet("superiors/{userId?}/by-type/{type}")]
    public async Task<ActionResult<IReadOnlyCollection<UserSuperiorDto>>> 
        GetSuperiorsByType(Guid? userId, RelationshipType type)
    
    // GET /api/hierarchy/subordinates/{userId?}
    [HttpGet("subordinates/{userId?}")]
    public async Task<ActionResult<IReadOnlyCollection<UserSubordinateDto>>> 
        GetSubordinates(Guid? userId)
    
    // GET /api/hierarchy/subordinates/{userId?}/all
    [HttpGet("subordinates/{userId?}/all")]
    public async Task<ActionResult<IReadOnlyCollection<UserSubordinateDto>>> 
        GetAllSubordinates(Guid? userId)
    
    // GET /api/hierarchy/chain/{userId?}
    [HttpGet("chain/{userId?}")]
    public async Task<ActionResult<IReadOnlyCollection<UserSuperiorDto>>> 
        GetCommandChain(Guid? userId)
    
    // ==========================================
    // ÁRBOL JERÁRÁRQUICO
    // ==========================================
    
    // GET /api/hierarchy/tree
    [HttpGet("tree")]
    [Authorize(Policy = "CanViewSubordinates")]
    public async Task<ActionResult<HierarchyTreeDto>> GetOrganizationTree()
    
    // GET /api/hierarchy/tree/full
    [HttpGet("tree/full")]
    [Authorize(Policy = "CanViewSubordinates")]
    public async Task<ActionResult<HierarchyTreeDto>> GetFullOrganizationTree()
    
    // ==========================================
    // UTILIDADES
    // ==========================================
    
    // GET /api/hierarchy/can-command/{commanderId}/{targetId}
    [HttpGet("can-command/{commanderId}/{targetId}")]
    public async Task<ActionResult<bool>> CanCommand(Guid commanderId, Guid targetId)
    
    // GET /api/hierarchy/level/{userId?}
    [HttpGet("level/{userId?}")]
    public async Task<ActionResult<int>> GetHierarchyLevel(Guid? userId)
    
    // POST /api/hierarchy/sync-primary/{userId}
    [HttpPost("sync-primary/{userId}")]
    [Authorize(Policy = "CanManageHierarchy")]
    public async Task<IActionResult> SyncPrimaryReportsTo(Guid userId)
}
```

### 5. DTOs

```csharp
// ==========================================
// RELACIONES
// ==========================================

public record UserReportingRelationshipDto(
    UserReportingRelationshipId Id,
    UserId UserId,
    string UserName,
    UserId ReportsToUserId,
    string ReportsToUserName,
    RelationshipType Type,
    int Priority,
    bool IsActive,
    DateTime CreatedAtUtc);

public record CreateRelationshipRequest(
    UserId UserId,
    UserId ReportsToUserId,
    RelationshipType Type,
    int Priority = 1);

public record UpdatePriorityRequest(
    int NewPriority);

// ==========================================
// SUPERIORES Y SUBORDINADOS
// ==========================================

public record UserSubordinateDto(
    UserId UserId,
    string UserName,
    string FullName,
    int HierarchyLevel,
    string RoleName,
    bool IsDirectSubordinate,
    IReadOnlyCollection<RelationshipType> RelationshipTypes);  // Tipos de relación

public record UserSuperiorDto(
    UserId UserId,
    string UserName,
    string FullName,
    int HierarchyLevel,
    string RoleName,
    RelationshipType RelationshipType,  // Tipo de esta relación
    int Priority);                      // Prioridad de esta relación

// ==========================================
// ÁRBOL JERÁRQUICO
// ==========================================

public record HierarchyTreeDto(
    UserId RootUserId,
    string RootUserName,
    int RootLevel,
    IReadOnlyCollection<HierarchyTreeNodeDto> Children);

public record HierarchyTreeNodeDto(
    UserId UserId,
    string UserName,
    string FullName,
    int HierarchyLevel,
    string RoleName,
    IReadOnlyCollection<HierarchyTreeNodeDto> Children,
    IReadOnlyCollection<SecondaryRelationshipDto>? SecondaryRelationships);  // Relaciones secundarias (Project, Matrix, etc.)

public record SecondaryRelationshipDto(
    UserId UserId,
    string UserName,
    RelationshipType Type,
    int Priority);

// ==========================================
// AUTHORIZATION
// ==========================================

public record HierarchyRequirement : IAuthorizationRequirement
{
    public Guid TargetUserId { get; init; }
    public RelationshipType? RelationshipType { get; init; }
}
```

### 6. Migración de Datos

#### 6.1 Roles Seed Actualizado
```json
{
  "roles": [
    { "name": "MasterAdmin", "level": 100, "isSystem": true },
    { "name": "Admin", "level": 90, "isSystem": true },
    { "name": "Director", "level": 80, "isSystem": false },
    { "name": "Manager", "level": 70, "isSystem": false },
    { "name": "Coordinator", "level": 60, "isSystem": false },
    { "name": "Supervisor", "level": 50, "isSystem": false },
    { "name": "Operator", "level": 40, "isSystem": false },
    { "name": "Worker", "level": 30, "isSystem": false },
    { "name": "Client", "level": 20, "isSystem": false },
    { "name": "User", "level": 10, "isSystem": false }
  ]
}
```

#### 6.2 Migración EF Core
```csharp
// 1. Agregar columnas a Role
modelBuilder.Entity<Role>()
    .Property(r => r.Level)
    .HasDefaultValue(10);

modelBuilder.Entity<Role>()
    .Property(r => r.ParentRoleId)
    .IsRequired(false);

// 2. Renombrar ReportsToUserId en TenantUser (backward compatibility)
modelBuilder.Entity<TenantUser>()
    .Property(tu => tu.ReportsToUserId)
    .HasColumnName("PrimaryReportsToUserId")  // Renombrar para claridad
    .IsRequired(false);

modelBuilder.Entity<TenantUser>()
    .Property(tu => tu.HierarchyLevel)
    .HasDefaultValue(10);

// 3. Nueva tabla: UserReportingRelationship
modelBuilder.Entity<UserReportingRelationship>(entity =>
{
    entity.HasKey(e => e.Id);
    
    entity.Property(e => e.UserId)
        .IsRequired();
    
    entity.Property(e => e.ReportsToUserId)
        .IsRequired();
    
    entity.Property(e => e.TenantId)
        .IsRequired();
    
    entity.Property(e => e.Type)
        .HasConversion<string>()
        .HasMaxLength(50);
    
    entity.Property(e => e.Priority)
        .HasDefaultValue(1);
    
    entity.Property(e => e.IsActive)
        .HasDefaultValue(true);
    
    entity.Property(e => e.CreatedAtUtc)
        .HasDefaultValueSql("NOW()");
    
    // Relaciones
    entity.HasOne(e => e.User)
        .WithMany()
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasOne(e => e.ReportsToUser)
        .WithMany()
        .HasForeignKey(e => e.ReportsToUserId)
        .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasOne(e => e.Tenant)
        .WithMany()
        .HasForeignKey(e => e.TenantId)
        .OnDelete(DeleteBehavior.Restrict);
    
    // Índices
    entity.HasIndex(e => new { e.UserId, e.TenantId });
    entity.HasIndex(e => new { e.ReportsToUserId, e.TenantId });
    entity.HasIndex(e => new { e.TenantId, e.Type });
    entity.HasIndex(e => new { e.UserId, e.TenantId, e.Type })
        .IsUnique();  // Un usuario solo puede tener una relación de cada tipo por tenant
});

// 4. Nueva tabla: RelationshipAuditLog
modelBuilder.Entity<RelationshipAuditLog>(entity =>
{
    entity.HasKey(e => e.Id);
    
    entity.Property(e => e.Action)
        .HasMaxLength(50);
    
    entity.Property(e => e.Reason)
        .HasMaxLength(500);
    
    entity.HasIndex(e => new { e.UserId, e.TenantId });
    entity.HasIndex(e => e.ChangedAtUtc);
});

// 5. Índices actualizados
modelBuilder.Entity<TenantUser>()
    .HasIndex(tu => new { tu.TenantId, tu.ReportsToUserId });

modelBuilder.Entity<Role>()
    .HasIndex(r => r.Level);
```

### 7. Flujo de Uso

#### 7.1 Crear Usuario con Superior Principal
```csharp
// 1. Crear usuario
var user = await createUserCommandHandler.HandleAsync(createUserCommand);

// 2. Asignar a tenant con superior funcional (principal)
await hierarchyService.CreateRelationshipAsync(
    userId: user.Id,
    reportsToUserId: managerUserId,  // El manager funcional
    tenantId: tenantId,
    type: RelationshipType.Functional,
    priority: 1);  // Principal

// 3. (Opcional) Asignar líder de proyecto
await hierarchyService.CreateRelationshipAsync(
    userId: user.Id,
    reportsToUserId: projectLeadUserId,
    tenantId: tenantId,
    type: RelationshipType.Project,
    priority: 2);  // Secundario
```

#### 7.2 Delegar Tareas (Ejemplo)
```csharp
// 1. Obtener todos los subordinados del manager funcional
var subordinates = await hierarchyService.GetDirectSubordinatesAsync(
    managerUserId, tenantId);

// 2. Filtrar por nivel (solo coordinadores y operadores)
var coordinators = subordinates
    .Where(s => s.HierarchyLevel >= 40 && s.HierarchyLevel <= 60);

// 3. Asignar tarea
foreach (var coordinator in coordinators)
{
    await taskService.AssignTaskAsync(taskId, coordinator.UserId);
}

// 4. Obtener subordinados por tipo de relación (ej: solo los de proyecto)
var projectSubordinates = await hierarchyService.GetDirectSubordinatesAsync(
    managerUserId, tenantId, RelationshipType.Project);
```

#### 7.3 Verificar Autorización (Múltiples Superiores)
```csharp
// ¿Puede el ManagerA (funcional) editar al operador?
var canEditFuncional = await hierarchyService.CanCommandAsync(
    commanderUserId: managerAUserId,
    targetUserId: operatorUserId,
    tenantId: tenantId);
// Resultado: true (es su superior funcional)

// ¿Puede el ManagerB (proyecto) editar al operador?
var canEditProject = await hierarchyService.CanCommandAsync(
    commanderUserId: managerBUserId,
    targetUserId: operatorUserId,
    tenantId: tenantId);
// Resultado: true (es su superior de proyecto)

// ¿Puede el ManagerC (sin relación) editar al operador?
var canEditOther = await hierarchyService.CanCommandAsync(
    commanderUserId: managerCUserId,
    targetUserId: operatorUserId,
    tenantId: tenantId);
// Resultado: false (no tiene relación)

// Verificar por tipo específico
var canEditByType = await hierarchyService.CanCommandByTypeAsync(
    commanderUserId: managerAUserId,
    targetUserId: operatorUserId,
    tenantId: tenantId,
    RelationshipType.Functional);
// Resultado: true
```

#### 7.4 Gestión de Árbol Matrix
```csharp
// Obtener árbol completo (muestra todas las relaciones)
var fullTree = await hierarchyService.GetFullOrganizationTreeAsync(tenantId);

// Ejemplo de respuesta para un nodo:
// {
//   userId: "operator-1",
//   userName: "Juan Pérez",
//   hierarchyLevel: 40,
//   roleName: "Operator",
//   children: [],
//   secondaryRelationships: [
//     { userId: "manager-b", userName: "María García", type: "Project", priority: 2 },
//     { userId: "coordinator-c", userName: "Carlos López", type: "Matrix", priority: 3 }
//   ]
// }

// Obtener árbol principal (solo Functional relationships)
var mainTree = await hierarchyService.GetOrganizationTreeAsync(tenantId);
// Solo muestra la jerarquía funcional principal
```

#### 7.5 Sincronización de PrimaryReportsToUserId
```csharp
// Cuando se crea/actualiza una relación Functional con prioridad 1,
// se sincroniza automáticamente con TenantUser.PrimaryReportsToUserId

// Sincronización manual (si es necesario)
await hierarchyService.SyncPrimaryReportsToAsync(userId, tenantId);

// Esto garantiza backward compatibility con código existente que usa:
// var primarySuperiorId = tenantUser.PrimaryReportsToUserId;
```

### 8. UI/Componentes Blazor

#### 8.1 Componente Árbol Jerárquico
```razor
@* Components/Hierarchy/OrganizationTree.razor *@

<HierarchyTree TenantId="@CurrentTenantId" OnUserSelected="@HandleUserSelected">
    <NodeTemplate>
        <div class="hierarchy-node level-@context.HierarchyLevel">
            <span class="user-name">@context.FullName</span>
            <span class="role-badge">@context.RoleName</span>
        </div>
    </NodeTemplate>
</HierarchyTree>
```

#### 8.2 Página de Gestión de Jerarquía
```razor
@* Pages/Hierarchy/Manage.razor *@

@page "/hierarchy/manage"

<h3>Organigrama</h3>

<div class="hierarchy-container">
    <OrganizationTree TenantId="@TenantId" 
                      OnUserSelected="@ShowUserDetails" />
    
    <UserDetailsPanel User="@SelectedUser" 
                      OnReassign="@ReassignUser" />
</div>
```

### 9. Consideraciones de Rendimiento

1. **Cache del Árbol**: El árbol jerárquico no cambia frecuentemente, usar cache
2. **CTEs Recursivos**: Para queries de "todos los subordinados", usar PostgreSQL recursive CTEs
3. **Índices**: Crear índices en `(TenantId, ReportsToUserId)` y `(TenantId, HierarchyLevel)`

### 10. Seguridad

#### 10.1 Detección de Ciclos (CRÍTICO con múltiples relaciones)
```csharp
// En HierarchyService.CreateRelationshipAsync()
private async Task<bool> WouldCreateCycleAsync(
    UserId userId, UserId reportsToUserId, TenantId tenantId)
{
    // Obtener todos los superiores del nuevo superior recursivamente
    // Si el usuario actual aparece en esa cadena, crearía un ciclo
    
    var chain = await GetCommandChainAsync(reportsToUserId, tenantId);
    return chain.Any(s => s.UserId == userId);
}

// Ejemplo de ciclo inválido:
// A reporta a B
// B reporta a C
// C intenta reportar a A ← CICLO DETECTADO
```

#### 10.2 Límite de Profundidad
```csharp
// Configurable por tenant o global
public class HierarchyOptions
{
    public int MaxDepth { get; set; } = 10;  // Máximo 10 niveles
    public int MaxSubordinatesPerUser { get; set; } = 100;
    public int MaxSuperiorsPerUser { get; set; } = 5;  // Máximo 5 superiores
}

// Validación en CreateRelationshipAsync()
if (superiors.Count >= _options.MaxSuperiorsPerUser)
    throw new ValidationException(
        $"Un usuario no puede tener más de {_options.MaxSuperiorsPerUser} superiores");
```

#### 10.3 Auditoría
```csharp
// Todas las operaciones registran en RelationshipAuditLog
await _context.RelationshipAuditLogs.AddAsync(new RelationshipAuditLog
{
    UserId = userId,
    TenantId = tenantId,
    ReportsToUserId = reportsToUserId,
    Type = type,
    Action = "Created",
    ChangedAtUtc = DateTime.UtcNow,
    ChangedByUserId = currentUserId,
    Reason = reason
});
```

#### 10.4 Reglas de Negocio
1. **Un usuario no puede ser su propio superior** (directa o indirectamente)
2. **Un usuario no puede tener más de N superiores** (configurable)
3. **Las relaciones deben ser within del mismo tenant**
4. **No se pueden crear relaciones circulares**
5. **Solo usuarios con rol ≥ Coordinator pueden asignar relaciones matrix**

## Implementación - Orden Sugerido

### Fase 1: Modelo de Datos (3-4 días)
1. [ ] Extender `Role` con `Level` y `ParentRoleId`
2. [ ] Crear entity `UserReportingRelationship` con tipos
3. [ ] Crear entity `RelationshipAuditLog`
4. [ ] Renombrar `TenantUser.ReportsToUserId` → `PrimaryReportsToUserId`
5. [ ] Crear migración EF Core
6. [ ] Actualizar seed data (roles con niveles)
7. [ ] Unit tests para validación de modelos

### Fase 2: Servicios Core (4-5 días)
1. [ ] Implementar `IHierarchyService` interface completa
2. [ ] Implementar `HierarchyService` - Gestión de relaciones
3. [ ] Implementar `HierarchyService` - CTEs recursivos PostgreSQL
4. [ ] Implementar detección de ciclos
5. [ ] Implementar sincronización `PrimaryReportsToUserId`
6. [ ] Unit tests completos

### Fase 3: API (3-4 días)
1. [ ] Crear `HierarchyController` con todos los endpoints
2. [ ] Implementar CRUD de relaciones
3. [ ] Implementar consultas jerárquicas
4. [ ] Agregar authorization policies matrix
5. [ ] Integration tests

### Fase 4: Integración (3-4 días)
1. [ ] Actualizar `AdminPasswordSignInService` con claims de jerarquía
2. [ ] Crear `HierarchyAuthorizationHandler`
3. [ ] Integrar con sistema de tareas existente
4. [ ] Actualizar UI existente para mostrar jerarquía
5. [ ] Backward compatibility con código existente

### Fase 5: UI (4-5 días)
1. [ ] Crear componente `OrganizationTree` (visualiza relaciones secundarias)
2. [ ] Crear componente `RelationshipManager` (CRUD de relaciones)
3. [ ] Crear página de gestión de jerarquía matrix
4. [ ] Agregar drag-and-drop para reasignar
5. [ ] Filtros por tipo de relación/nivel/rol
6. [ ] Vista de árbol principal vs árbol completo

### Fase 6: Optimización (2-3 días)
1. [ ] Implementar cache del árbol jerárquico
2. [ ] Optimizar CTEs recursivos
3. [ ] Índices de base de datos
4. [ ] Performance testing con datos reales

**Total Estimado: 19-25 días**

## Alternativas Consideradas

### Alternativa A: Solo Role Hierarchy (Rechazada)
- Pros: Simple, menos cambios
- Cons: No permite "quién reporta a quién" por usuario, solo por rol

### Alternativa B: User Hierarchy Simple (1 superior por usuario)
- Pros: Simple, árbol claro
- Cons: No soporta organizaciones matrix

### Alternativa C: Hybrid con ReportsToUserId en TenantUser (Rechazada)
- Pros: Simple, backward compatible
- Cons: Solo 1 superior por usuario, no soporta matrix

### Alternativa D: UserReportingRelationship con Tipos (Seleccionada) ✓
- Pros: Soporta 1 o múltiples superiores, flexible, escalable
- Cons: Más complejo de implementar, requiere CTEs recursivos más avanzados

**Justificación de la selección:**
- Soporta ambos casos (simple y matrix)
- Permite tipos de relación (funcional, proyecto, matrix, mentor, temporal)
- Prioridad para definir quién es principal vs secundario
- Backward compatible con código existente
- Escalable para el futuro

## Diagrama de Entidades

```
┌─────────────────────────────────────────────────────────────┐
│                         ROLE                                │
├─────────────────────────────────────────────────────────────┤
│ Id: RoleId                                                  │
│ Name: RoleName                                              │
│ Level: int (10-100) ← NUEVO                                 │
│ ParentRoleId: RoleId? ← NUEVO                               │
│ IsActive: bool                                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ (1)
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      ROLE PERMISSION                        │
├─────────────────────────────────────────────────────────────┤
│ RoleId                                                      │
│ PermissionId                                                │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   USER REPORTING RELATIONSHIP ← NUEVA       │
├─────────────────────────────────────────────────────────────┤
│ Id: UserReportingRelationshipId                             │
│ UserId ← Quién reporta                                     │
│ ReportsToUserId ← A quién reporta                          │
│ TenantId ← Scoped por tenant                               │
│ Type: RelationshipType (Functional/Project/Matrix/etc)      │
│ Priority: int (1=Principal, 2+=Secundario)                  │
│ IsActive: bool                                              │
│ CreatedAtUtc: DateTime                                      │
└─────────────────────────────────────────────────────────────┘
       │                         ▲
       │ (N)                     │ (N)
       ▼                         │
┌─────────────────────┐    ┌─────────────────────┐
│       USER          │    │       USER          │
│ (Quién reporta)     │    │ (A quién reporta)   │
└─────────────────────┘    └─────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                      TENANT USER                            │
├─────────────────────────────────────────────────────────────┤
│ Id: TenantUserId                                            │
│ TenantId                                                    │
│ UserId                                                      │
│ PrimaryReportsToUserId: UserId? (Sync con Functional P1)    │
│ HierarchyLevel: int ← NUEVO                                 │
│ IsActive: bool                                              │
│ JoinedAtUtc: DateTime                                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ (1)
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                          USER                               │
├─────────────────────────────────────────────────────────────┤
│ Id: UserId                                                  │
│ Name, LastName, Email, etc. (existente)                     │
│ TenantId (home tenant)                                      │
│ SecurityUserId                                              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│               RELATIONSHIP AUDIT LOG ← NUEVA                │
├─────────────────────────────────────────────────────────────┤
│ Id: RelationshipAuditLogId                                  │
│ UserId                                                      │
│ TenantId                                                    │
│ ReportsToUserId                                             │
│ Type: RelationshipType                                      │
│ Action: string (Created/Updated/Deleted)                    │
│ ChangedAtUtc: DateTime                                      │
│ ChangedByUserId: UserId                                     │
│ Reason: string?                                             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    USER ROLE (existente)                     │
├─────────────────────────────────────────────────────────────┤
│ UserId                                                      │
│ RoleId                                                      │
└─────────────────────────────────────────────────────────────┘
```

### Ejemplo de Datos

**Caso 1: Un solo jefe (árbol simple)**
```
UserReportingRelationship:
┌──────────┬───────────────┬──────────┬─────────────┬──────────┐
│ UserId   │ ReportsToId   │ TenantId │ Type        │ Priority │
├──────────┼───────────────┼──────────┼─────────────┼──────────┤
│ Operator │ Coordinator   │ T1       │ Functional  │ 1        │
│ Coord    │ Manager       │ T1       │ Functional  │ 1        │
│ Manager  │ Director      │ T1       │ Functional  │ 1        │
└──────────┴───────────────┴──────────┴─────────────┴──────────┘
```

**Caso 2: Varios jefes (organización matrix)**
```
UserReportingRelationship:
┌──────────┬───────────────┬──────────┬─────────────┬──────────┐
│ UserId   │ ReportsToId   │ TenantId │ Type        │ Priority │
├──────────┼───────────────┼──────────┼─────────────┼──────────┤
│ Operator │ ManagerA      │ T1       │ Functional  │ 1        │  ← Jefe funcional
│ Operator │ ManagerB      │ T1       │ Project     │ 2        │  ← Líder proyecto
│ Operator │ Coordinator   │ T1       │ Matrix      │ 3        │  ← Coordinación
└──────────┴───────────────┴──────────┴─────────────┴──────────┘

TenantUser (sync):
┌──────────┬───────────────┬──────────────────────┬─────────┐
│ UserId   │ TenantId      │ PrimaryReportsToId   │ Level   │
├──────────┼───────────────┼──────────────────────┼─────────┤
│ Operator │ T1            │ ManagerA (sync)      │ 40      │
└──────────┴───────────────┴──────────────────────┴─────────┘
```

## Queries de Ejemplo (PostgreSQL)

### Obtener todos los subordinados recursivamente
```sql
WITH RECURSIVE subordinates AS (
    -- Base case: subordinados directos
    SELECT tu.UserId, tu.ReportsToUserId, tu.HierarchyLevel, u.UserName
    FROM TenantUser tu
    JOIN "User" u ON tu.UserId = u.Id
    WHERE tu.ReportsToUserId = @UserId 
      AND tu.TenantId = @TenantId
      AND tu.IsActive = true
    
    UNION ALL
    
    -- Caso recursivo: subordinados de subordinados
    SELECT tu2.UserId, tu2.ReportsToUserId, tu2.HierarchyLevel, u2.UserName
    FROM TenantUser tu2
    JOIN "User" u2 ON tu2.UserId = u2.Id
    JOIN subordinates s ON tu2.ReportsToUserId = s.UserId
    WHERE tu2.TenantId = @TenantId
      AND tu2.IsActive = true
)
SELECT * FROM subordinates;
```

### Obtener cadena de mando (hasta la raíz)
```sql
WITH RECURSIVE chain AS (
    -- Base case: superior directo
    SELECT tu.UserId, tu.ReportsToUserId, tu.HierarchyLevel, u.UserName
    FROM TenantUser tu
    JOIN "User" u ON tu.ReportsToUserId = u.Id
    WHERE tu.UserId = @UserId 
      AND tu.TenantId = @TenantId
    
    UNION ALL
    
    -- Caso recursivo: superior del superior
    SELECT tu2.UserId, tu2.ReportsToUserId, tu2.HierarchyLevel, u2.UserName
    FROM TenantUser tu2
    JOIN "User" u2 ON tu2.ReportsToUserId = u2.Id
    JOIN chain c ON tu2.UserId = c.ReportsToUserId
    WHERE tu2.TenantId = @TenantId
)
SELECT * FROM chain;
```

## Casos de Uso: Cuándo Usar Cada Modelo

### Caso 1: Un Solo Jefe (Árbol Simple)
**Cuándo usar:**
- Organizaciones pequeñas/medianas
- Estructura jerárquica estricta
- Cadena de mando clara (militar, gubernamental)
- Un empleado tiene un solo responsable

**Ejemplo:**
```
Gerente General
    ├── Gerente Ventas
    │       ├── Supervisor Call Center
    │       │       └── Agentes
    │       └── Supervisor Retail
    │               └── Vendedores
    └── Gerente Operaciones
            └── Coordinador Logística
                    └── Repartidores
```

**Configuración:**
```csharp
// Un solo Functional relationship por usuario
await hierarchyService.CreateRelationshipAsync(
    userId: operatorId,
    reportsToUserId: coordinatorId,
    tenantId: tenantId,
    type: RelationshipType.Functional,
    priority: 1);
```

### Caso 2: Varios Jefes (Organización Matrix)
**Cuándo usar:**
- Empresas de proyectos (consultoría, TI, construcción)
- Organizaciones矩阵ales
- Usuarios que trabajan en múltiples proyectos/áreas
- Necesidad de supervisión especializada

**Ejemplo:**
```
Director TI (Funcional)
    │
    └── Líder Proyecto Alpha (Proyecto)
            │
            └── Developer (reporta a AMBOS)
                    ├── Funcional: Director TI (evaluación anual)
                    └── Proyecto: Líder Alpha (entregables diarios)
```

**Configuración:**
```csharp
// Relación funcional (principal)
await hierarchyService.CreateRelationshipAsync(
    userId: developerId,
    reportsToUserId: directorTIId,
    tenantId: tenantId,
    type: RelationshipType.Functional,
    priority: 1);

// Relación de proyecto (secundaria)
await hierarchyService.CreateRelationshipAsync(
    userId: developerId,
    reportsToUserId: projectLeaderId,
    tenantId: tenantId,
    type: RelationshipType.Project,
    priority: 2);
```

### Caso 3: Mixto (Algunos usuarios 1 jefe, otros múltiples)
**Este es el caso más común en empresas reales.**

```
Director General
    ├── Gerente Ventas (1 jefe: Director)
    │       ├── Vendedor A (1 jefe: Gerente Ventas)
    │       └── Vendedor B (2 jefes: Gerente Ventas + Líder Proyecto X)
    └── Gerente Proyectos (1 jefe: Director)
            └── Líder Proyecto X (2 jefes: Gerente Proyectos + Gerente Ventas)
                    └── Developer (2 jefes: Líder X + Director TI)
```

**Configuración:**
```csharp
// Para Vendedor A (simple)
await hierarchyService.CreateRelationshipAsync(
    userId: vendedorAId,
    reportsToUserId: gerenteVentasId,
    tenantId: tenantId,
    type: RelationshipType.Functional,
    priority: 1);

// Para Vendedor B (matrix)
await hierarchyService.CreateRelationshipAsync(
    userId: vendedorBId,
    reportsToUserId: gerenteVentasId,
    tenantId: tenantId,
    type: RelationshipType.Functional,
    priority: 1);

await hierarchyService.CreateRelationshipAsync(
    userId: vendedorBId,
    reportsToUserId: liderProyectoXId,
    tenantId: tenantId,
    type: RelationshipType.Project,
    priority: 2);
```

### Recomendación

**Empieza con el modelo mixto** porque:
1. Soporta ambos casos (simple y matrix)
2. Es más flexible para el futuro
3. No complica el caso simple (solo 1 relación)
4. Permite crecer sin reescribir código

## Notas Importantes

1. **Multi-tenancy**: La jerarquía es por tenant. Un usuario puede ser Manager en Tenant A y Operator en Tenant B.

2. **Roles vs Jerarquía**: Los roles definen PERMISOS, la jerarquía define QUIÉN REPORTA A QUIÉN. Pueden divergir (un usuario con rol Manager podría reportar a un Director con rol Coordinator en un tenant específico).

3. **Performance**: Los CTEs recursivos son eficientes en PostgreSQL pero pueden ser lentos con miles de usuarios. Considerar cache para el árbol completo.

4. **Validación**: Siempre validar que no se creen ciclos al reasignar superiores.

5. **Backward Compatibility**: Se mantiene `TenantUser.PrimaryReportsToUserId` sincronizado con la relación `Functional` prioridad 1 para código existente.

6. **Máximo de Superiores**: Configurar un límite (ej: 5) para evitar abusos del modelo matrix.

## Próximos Pasos

¿Te gustaría que proceda con la implementación? Puedo comenzar con:
1. **Fase 1**: Modelo de datos y migración
2. **Fase 2**: Servicios core

O si prefieres, puedo profundizar en algún aspecto específico antes de implementar.
