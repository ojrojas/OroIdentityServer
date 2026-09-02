## Context

El proyecto usa arquitectura DDD con CQRS, OpenIddict para autenticación, PostgreSQL como base de datos, y Blazor para UI. El sistema actual tiene roles planos (`Administrator`, `Manager`, `User`) sin jerarquía definida. La multi-tenancy ya existe con `TenantUser` como join entity.

## Goals / Non-Goals

**Goals:**
- Implementar modelo de datos para relaciones de reporte (quién reporta a quién)
- Soportar múltiples superiores por usuario (organización matrix)
- Mantener backward compatibility con `TenantUser.PrimaryReportsToUserId`
- Proporcionar consultas jerárquicas recursivas eficientes
- Implementar autorización basada en jerarquía

**Non-Goals:**
- No cambiar el sistema de autenticación existente (solo agregar claims)
- No implementar UI completa (solo componentes base)
- No migrar datos existentes (solo estructura)
- No cambiar la lógica de permisos existente (solo agregar policies de jerarquía)

## Decisions

### Decision 1: Entity UserReportingRelationship en lugar de ReportsToUserId en TenantUser

**Opción A**: Agregar `ReportsToUserId` a `TenantUser`
- Pros: Simple, 1 relación por usuario
- Cons: No soporta organización matrix

**Opción B**: Crear entity `UserReportingRelationship` (SELECCIONADA)
- Pros: Soporta múltiples superiores, tipos de relación, prioridad, auditoría
- Cons: Más complejo, requiere CTEs recursivos

**Justificación**: El requisito de soporte matrix (usuario con múltiples jefes) es crítico. El modelo de entity separada es más extensible y mantiene backward compatibility.

### Decision 2: CTEs Recursivos PostgreSQL

**Opción A**: LINQ con recursion en memoria
- Pros: Simple de implementar
- Cons: Ineficiente con muchos usuarios, carga todo en memoria

**Opción B**: CTEs recursivos en PostgreSQL (SELECCIONADA)
- Pros: Eficiente, ejecuta en la DB, maneja millones de registros
- Cons: Más complejo de escribir, dependiente de PostgreSQL

**Justificación**: PostgreSQL soporta CTEs recursivos nativamente. Para miles de usuarios, la performance es crítica.

### Decision 3: Sync de PrimaryReportsToUserId

**Opción A**: No sincronizar, usar solo UserReportingRelationship
- Pros: Simple, una sola fuente de verdad
- Cons: Rompe código existente que usa TenantUser.ReportsToUserId

**Opción B**: Sincronizar automáticamente (SELECCIONADA)
- Pros: Backward compatibility, código existente funciona
- Cons: Duplicación de datos, riesgo de inconsistencia

**Justificación**: La migración de código existente es riesgosa. La sincronización automática mantiene compatibilidad mientras se migra gradualmente.

### Decision 4: Claims en autenticación

**Opción A**: Claims estáticos en seed data
- Pros: Simple
- Cons: No refleja cambios dinámicos de jerarquía

**Opción B**: Claims dinámicos en BuildPrincipalAsync (SELECCIONADA)
- Pros: Siempre actualizados, reflejan jerarquía real
- Cons: Overhead en cada login (mitigado con cache)

**Justificación**: La jerarquía cambia con frecuencia. Los claims deben reflejar el estado actual.

### Decision 5: Auditoría en entity separada

**Opción A**: Log en tabla genérica
- Pros: Reutilizable
- Cons: Menos específico, más difícil de query

**Opción B**: Entity `RelationshipAuditLog` específica (SELECCIONADA)
- Pros: Específica para jerarquía, fácil de query, campos relevantes
- Cons: Una entity más

**Justificación**: La auditoría de jerarquía requiere campos específicos (type, action, reason). Entity dedicada es más limpia.

## Risks / Trade-offs

**Risk**: CTEs recursivos pueden ser lentos con miles de usuarios
→ **Mitigation**: Implementar cache del árbol jerárquico, índices en `(TenantId, ReportsToUserId)`, límite de profundidad configurable

**Risk**: Sincronización PrimaryReportsToUserId puede causar inconsistencias
→ **Mitigation**: Transacciones atómicas, validación en ambos lados, endpoint de sync manual

**Risk**: Múltiples superiores pueden crear ciclos complejos
→ **Mitigation**: Detección de ciclos en CreateRelationshipAsync, límite de profundidad, validación antes de crear

**Risk**: Cambios en jerarquía afectan autorización en tiempo real
→ **Mitigation**: Claims se actualizan en login, fallback a verificación en tiempo de ejecución

**Risk**: Backward compatibility con código existente
→ **Mitigation**: Sync automático, deprecation warnings, migración gradual

## Migration Plan

1. **Pre-deployment**: Crear nuevas tablas y columnas (migración EF Core)
2. **Deployment**: Actualizar código (nuevos services, controllers, claims)
3. **Post-deployment**: Sincronizar datos existentes (TenantUser.PrimaryReportsToUserId)
4. **Rollback**: Revertir migración y código (nuevas tablas se pueden droppear)

## Open Questions

- ¿Cuántos usuarios máximo se esperan por tenant? (afecta tamaño de cache)
- ¿Se requiere notificación cuando cambia la jerarquía? (para sistemas downstream)
- ¿Se necesita exportar el árbol jerárquico? (reportes)
