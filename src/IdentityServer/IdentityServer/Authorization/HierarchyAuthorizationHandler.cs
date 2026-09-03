// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OroIdentityServer.Core.Modules.Hierarchy.Enums;
using OroIdentityServer.Core.Modules.Hierarchy.Services;
using OroIdentityServer.Core.Shared;
using OroIdentityServer.Shared.Authorization;

namespace OroIdentityServer.Server.Authorization;

public sealed class HierarchyAuthorizationHandler : AuthorizationHandler<HierarchyRequirement>
{
    private readonly IHierarchyService _hierarchyService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HierarchyAuthorizationHandler> _logger;

    public HierarchyAuthorizationHandler(
        IHierarchyService hierarchyService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<HierarchyAuthorizationHandler> logger)
    {
        _hierarchyService = hierarchyService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HierarchyRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }

        var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(currentUserIdClaim, out var currentUserIdGuid))
        {
            context.Fail();
            return;
        }

        var currentUserId = new UserId(currentUserIdGuid);

        // Tenant resolution
        Guid? tenantIdGuid = null;
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var header) && Guid.TryParse(header.ToString(), out var headerId))
            tenantIdGuid = headerId;
        else
        {
            var tenantClaim = context.User.FindFirstValue(AuthorizationClaimTypes.TenantId) ?? context.User.FindFirstValue("tenant_id");
            if (Guid.TryParse(tenantClaim, out var claimId))
                tenantIdGuid = claimId;
        }

        if (tenantIdGuid == null)
        {
            _logger.LogWarning("HierarchyAuthorizationHandler: TenantId not found");
            context.Fail();
            return;
        }

        var tenantId = new TenantId(tenantIdGuid.Value);

        // Target user resolution: from route, query, or requirement
        string? targetIdStr = null;
        if (!string.IsNullOrEmpty(requirement.TargetUserIdClaimType))
        {
            targetIdStr = context.User.FindFirstValue(requirement.TargetUserIdClaimType);
        }

        // Try route values
        if (string.IsNullOrEmpty(targetIdStr) && httpContext.Request.RouteValues.TryGetValue("targetId", out var routeTarget))
            targetIdStr = routeTarget?.ToString();
        if (string.IsNullOrEmpty(targetIdStr) && httpContext.Request.RouteValues.TryGetValue("userId", out var routeUser))
            targetIdStr = routeUser?.ToString();
        if (string.IsNullOrEmpty(targetIdStr) && httpContext.Request.Query.TryGetValue("targetId", out var qTarget))
            targetIdStr = qTarget.ToString();
        if (string.IsNullOrEmpty(targetIdStr) && httpContext.Request.Query.TryGetValue("userId", out var qUser))
            targetIdStr = qUser.ToString();

        // If no target, we are checking general command authority? For simple policy without target, check level
        if (string.IsNullOrEmpty(targetIdStr))
        {
            // For policies like CanManageHierarchy that only check level, we already handle via other handler?
            // Fallback to checking hierarchy level claim
            var levelClaim = context.User.FindFirstValue(HierarchyClaimTypes.HierarchyLevel);
            if (int.TryParse(levelClaim, out var level))
            {
                // Requirement without target will be handled by level-based policy, not this handler
                // So succeed if we can determine? Instead fail to let other handlers decide
                context.Fail();
                return;
            }
            context.Fail();
            return;
        }

        if (!Guid.TryParse(targetIdStr, out var targetGuid))
        {
            context.Fail();
            return;
        }

        var targetId = new UserId(targetGuid);

        try
        {
            bool canCommand;
            if (!string.IsNullOrEmpty(requirement.RelationshipType) && Enum.TryParse<RelationshipType>(requirement.RelationshipType, true, out var relType))
            {
                canCommand = await _hierarchyService.CanCommandByTypeAsync(tenantId, currentUserId, targetId, relType, httpContext.RequestAborted);
            }
            else
            {
                canCommand = await _hierarchyService.CanCommandAsync(tenantId, currentUserId, targetId, httpContext.RequestAborted);
            }

            if (canCommand)
                context.Succeed(requirement);
            else
                context.Fail();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HierarchyAuthorizationHandler for {Current} -> {Target}", currentUserIdGuid, targetGuid);
            context.Fail();
        }
    }
}
