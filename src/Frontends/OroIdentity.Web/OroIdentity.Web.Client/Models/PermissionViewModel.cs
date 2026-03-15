using System.ComponentModel.DataAnnotations;

namespace OroIdentity.Web.Client.Models;

public class PermissionViewModel
{
    public IdViewModel PermissionId { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name must be at most 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Display name must be at most 200 characters")]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Resource { get; set; } = string.Empty;

    public bool IsSystem { get; set; }
}
