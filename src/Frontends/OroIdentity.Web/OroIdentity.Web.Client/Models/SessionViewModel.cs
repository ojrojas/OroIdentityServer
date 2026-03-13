namespace OroIdentity.Web.Client.Models;

public class SessionViewModel
{
    public IdViewModel Id { get; set; } = IdViewModel.New(Guid.Empty);
    public IdViewModel UserId { get; set; } = IdViewModel.Empty();
    public IdViewModel TenantId { get; set; } = IdViewModel.Empty();
    public string? AuthorizationId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }

    // Enriched display fields
    public string UserDisplayName { get; set; } = string.Empty;
}
