namespace OroIdentity.Web.Client.Models;

public class PermissionViewModel
{
    public IdViewModel PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Resource { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
}
