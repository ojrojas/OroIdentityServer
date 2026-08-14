using IdentityServer.Client.Models.Tenants;

namespace IdentityServer.Client.Services;

public interface ICurrentTenantContext
{
    Guid? CurrentTenantId { get; }
    TenantModel? CurrentTenant { get; }
    IReadOnlyList<TenantModel> Tenants { get; }
    bool IsMasterAdmin { get; }
    bool HasTenant(Guid tenantId);
    void Initialize(IEnumerable<TenantModel> tenants, Guid? preferredTenantId);
    void SetCurrentTenantId(Guid tenantId);
    void SetMasterAdmin(bool isMasterAdmin);
}

public sealed class CurrentTenantContext : ICurrentTenantContext
{
    private List<TenantModel> _tenants = [];

    public Guid? CurrentTenantId { get; private set; }

    public TenantModel? CurrentTenant =>
        CurrentTenantId is { } id ? _tenants.FirstOrDefault(t => t.Id == id) : null;

    public IReadOnlyList<TenantModel> Tenants => _tenants;

    public bool IsMasterAdmin { get; private set; }

    public bool HasTenant(Guid tenantId) => _tenants.Any(t => t.Id == tenantId);

    public void Initialize(IEnumerable<TenantModel> tenants, Guid? preferredTenantId)
    {
        _tenants = tenants.ToList();
        CurrentTenantId = preferredTenantId is { } id && _tenants.Any(t => t.Id == id)
            ? id
            : _tenants.FirstOrDefault()?.Id;
    }

    public void SetCurrentTenantId(Guid tenantId) => CurrentTenantId = tenantId;

    public void SetMasterAdmin(bool isMasterAdmin) => IsMasterAdmin = isMasterAdmin;
}
