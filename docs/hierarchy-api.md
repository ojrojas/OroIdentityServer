# Hierarchy API Documentation

Base path: `/api/hierarchy`
Authentication: Cookie (AdminScheme) - all endpoints require authenticated user; mutating endpoints require `CanManageHierarchy` (level ≥70).

## Relationships CRUD

### POST /api/hierarchy/relationships
Create a reporting relationship.
- **Auth:** `CanManageHierarchy`
- **Body:**
```json
{
  "userId": "guid",
  "reportsToUserId": "guid",
  "type": "Functional|Project|Matrix|Mentor|Temporary",
  "priority": 1
}
```
- **Responses:** `201 Created` with relationship, `400` on duplicate/cycle/limit/validation.

### PUT /api/hierarchy/relationships/{id}/priority
Update priority. Syncs `TenantUser.PrimaryReportsToUserId` if Functional priority changes to/from 1.
- **Auth:** `CanManageHierarchy`
- **Body:** `{ "priority": 2 }`
- **Responses:** `200 OK`, `404`, `400`.

### DELETE /api/hierarchy/relationships/{id}
Soft-delete (sets `IsActive=false`) and creates `RelationshipAuditLog` with `Deleted`.
- **Auth:** `CanManageHierarchy`
- **Responses:** `204 NoContent`, `404`.

### GET /api/hierarchy/relationships/{userId}
List all active relationships for a user in the current tenant.
- **Responses:** `200 OK` with `HierarchyRelationshipDto[]`.

## Superiors

### GET /api/hierarchy/superiors/{userId?}
Direct superiors. `userId` optional defaults to current user.
- **Responses:** `200 OK` with `SuperiorDto[]` (includes `hierarchyLevel`, `roleName`, `relationshipType`, `priority`).

### GET /api/hierarchy/superiors/{userId?}/primary
Primary superior (Functional priority 1). Returns `null` if none.
- **Responses:** `200 OK` with `SuperiorDto` or `null`.

### GET /api/hierarchy/superiors/{userId?}/by-type/{type}
Filter superiors by `RelationshipType`.
- **Responses:** `200 OK` with `SuperiorDto[]`, `400` on invalid type.

## Subordinates

### GET /api/hierarchy/subordinates/{userId?}?type=Functional
Direct subordinates, optional `type` filter.
- **Auth:** `CanViewSubordinates` (level ≥40)
- **Responses:** `200 OK` with `SubordinateDto[]`.

### GET /api/hierarchy/subordinates/{userId?}/all
All subordinates recursively (primary Functional path, CTE with depth limit `MaxDepth` default 10, visited array to prevent cycles).
- **Auth:** `CanViewSubordinates`
- **Responses:** `200 OK` with `SubordinateDto[]`.

## Command Chain & Authority

### GET /api/hierarchy/chain/{userId?}
Command chain from user up to root (primary path, CTE).
- **Responses:** `200 OK` with `SuperiorDto[]` (empty if root).

### GET /api/hierarchy/can-command/{commanderId}/{targetId}?type=Functional
Check authority. Without `type`, checks any relationship; with `type`, only that type is considered. Uses `GetAllSubordinates` plus direct check.
- **Responses:** `200 OK` with `{ canCommand: bool, commanderId, targetId }`.

## Tree

### GET /api/hierarchy/tree
Primary organization tree (Functional priority 1 only). Returns `OrganizationTreeNodeDto` with recursive children.
- **Responses:** `200 OK` with tree or `{ message: "No hierarchy found" }` if no relationships.

### GET /api/hierarchy/tree/full
Full tree with secondary relationships per node (`SecondaryRelationships` list).
- **Responses:** `200 OK` as above.

## Level & Sync

### GET /api/hierarchy/level/{userId?}
Effective hierarchy level (from `TenantUser.HierarchyLevel` or max `Role.Level`).
- **Responses:** `200 OK` with `{ userId, level }`.

### POST /api/hierarchy/sync-primary/{userId}
Sync `TenantUser.PrimaryReportsToUserId` from Functional priority 1 and update `HierarchyLevel` from role.
- **Auth:** `CanManageHierarchy`
- **Responses:** `200 OK` with `{ message: "Synced", userId }`.

## Claims (added at sign-in)

`AdminPasswordSignInService.BuildPrincipalAsync` injects:
- `hierarchy_level` – int 10–100
- `direct_superior_ids` – JSON array of Guid strings
- `primary_superior_id` – Guid (if Functional priority 1 exists)
- `relationship_types` – JSON array of RelationshipType names

## Policies

- `CanManageHierarchy` – level ≥70
- `CanViewSubordinates` – level ≥40
- `CanLeadProject` / `CanAssignMatrixRelationships` – level ≥60
- `HierarchyAuthorizationHandler` – runtime `CanCommand`/`CanCommandByType` verification via `IHierarchyService`

## Errors

Validation errors return `400` with `{ error: "..." }` containing messages like `cannot be own superior`, `cycle detected`, `relationship duplicate`, `maximum superiors exceeded`.

## Tenant Resolution

`X-Tenant-Id` header is preferred; fallback to `tenant_id` claim from cookie principal. All hierarchy operations are tenant-scoped.

## Configuration (`HierarchyOptions`)

```json
{
  "Hierarchy": {
    "MaxDepth": 10,
    "MaxSubordinatesPerUser": 1000,
    "MaxSuperiorsPerUser": 5
  }
}
```
