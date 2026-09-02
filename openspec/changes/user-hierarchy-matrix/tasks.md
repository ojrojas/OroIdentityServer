## 1. Data Model Setup

- [x] 1.1 Extend Role aggregate with Level (int, 10-100) and ParentRoleId properties, verify EF migration generates correctly
- [x] 1.2 Create RelationshipType enum (Functional, Project, Matrix, Mentor, Temporary)
- [x] 1.3 Create UserReportingRelationship entity with all properties and navigation properties, verify EF configuration
- [x] 1.4 Create RelationshipAuditLog entity with all properties, verify EF configuration
- [x] 1.5 Update TenantUser entity: rename ReportsToUserId to PrimaryReportsToUserId, add HierarchyLevel property
- [x] 1.6 Create EF migration for all new entities and modified properties, verify migration applies successfully
- [x] 1.7 Update seed data.json with roles and hierarchy levels

## 2. Core Services

- [x] 2.1 Create HierarchyOptions configuration class with MaxDepth, MaxSubordinatesPerUser, MaxSuperiorsPerUser
- [x] 2.2 Create IHierarchyService interface with all methods from design
- [x] 2.3 Implement CreateRelationshipAsync with cycle detection validation
- [x] 2.4 Implement UpdateRelationshipPriorityAsync with PrimaryReportsToUserId sync
- [x] 2.5 Implement DeleteRelationshipAsync with soft delete and audit logging
- [x] 2.6 Implement GetUserRelationshipsAsync and GetSuperiorsByTypeAsync
- [x] 2.7 Implement GetDirectSuperiorsAsync and GetPrimarySuperiorAsync
- [x] 2.8 Implement GetDirectSubordinatesAsync with relationship type filtering
- [x] 2.9 Implement GetAllSubordinatesAsync with recursive CTE PostgreSQL query
- [x] 2.10 Implement GetCommandChainAsync with recursive CTE PostgreSQL query
- [x] 2.11 Implement CanCommandAsync and CanCommandByTypeAsync for authority verification
- [x] 2.12 Implement GetOrganizationTreeAsync and GetFullOrganizationTreeAsync
- [x] 2.13 Implement GetHierarchyLevelAsync and SyncPrimaryReportsToAsync
- [x] 2.14 Create unit tests for HierarchyService covering all scenarios from specs

## 3. API Layer

- [x] 3.1 Create HierarchyController with DI registration
- [x] 3.2 Implement POST /api/hierarchy/relationships endpoint
- [x] 3.3 Implement PUT /api/hierarchy/relationships/{id}/priority endpoint
- [x] 3.4 Implement DELETE /api/hierarchy/relationships/{id} endpoint
- [x] 3.5 Implement GET /api/hierarchy/relationships/{userId} endpoint
- [x] 3.6 Implement GET /api/hierarchy/superiors/{userId?} endpoints (direct, primary, by-type)
- [x] 3.7 Implement GET /api/hierarchy/subordinates/{userId?} endpoints (direct, all)
- [x] 3.8 Implement GET /api/hierarchy/chain/{userId?} endpoint
- [x] 3.9 Implement GET /api/hierarchy/tree and /tree/full endpoints
- [x] 3.10 Implement GET /api/hierarchy/can-command/{commanderId}/{targetId} endpoint
- [x] 3.11 Implement GET /api/hierarchy/level/{userId?} endpoint
- [x] 3.12 Implement POST /api/hierarchy/sync-primary/{userId} endpoint
- [x] 3.13 Create integration tests for all API endpoints

## 4. Authorization Integration

- [x] 4.1 Add HierarchyClaimTypes constants class
- [x] 4.2 Update AdminPasswordSignInService.BuildPrincipalAsync to include hierarchy claims
- [x] 4.3 Create HierarchyRequirement authorization requirement class
- [x] 4.4 Create HierarchyAuthorizationHandler for runtime authority verification
- [x] 4.5 Add authorization policies (CanManageHierarchy, CanViewSubordinates, CanLeadProject, CanAssignMatrixRelationships) to CookieAuthHandlerSetup
- [x] 4.6 Register HierarchyAuthorizationHandler in DI container
- [x] 4.7 Create integration tests for authorization policies

## 5. UI Components

- [x] 5.1 Create OrganizationTree Blazor component with OnUserSelected callback
- [x] 5.2 Create RelationshipManager Blazor component for CRUD operations
- [x] 5.3 Create HierarchyManage page at /hierarchy/manage route
- [x] 5.4 Add hierarchy level and relationship type filters to tree view
- [x] 5.5 Add CSS styles for hierarchy nodes and relationship badges
- [x] 5.6 Create integration tests for UI components

## 6. Polish & Documentation

- [x] 6.1 Add XML documentation to all public interfaces and classes
- [x] 6.2 Update README with hierarchy system overview
- [x] 6.3 Create API documentation for hierarchy endpoints
- [x] 6.4 Run full test suite and fix any failures
- [x] 6.5 Verify all specs are implemented and pass acceptance criteria
