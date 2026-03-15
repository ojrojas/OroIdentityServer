using System.ComponentModel.DataAnnotations;

namespace OroIdentity.Web.Client.Models;

public record CreateTenantRequest([property: Required(ErrorMessage = "Name is required")][property: StringLength(100, ErrorMessage = "Name must be at most 100 characters")] string Name, bool IsActive);
public record UpdateTenantRequest([property: Required(ErrorMessage = "Name is required")][property: StringLength(100, ErrorMessage = "Name must be at most 100 characters")] string Name, bool IsActive);

public record GetTenantsResponse(IEnumerable<TenantViewModel>? Data = null);
public record GetTenantResponse(TenantViewModel? Data = null);

public record CreateTenantResponse(Guid? Id = null);
public record UpdateTenantResponse();
