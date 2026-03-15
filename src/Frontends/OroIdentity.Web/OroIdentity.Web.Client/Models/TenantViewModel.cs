namespace OroIdentity.Web.Client.Models;

public class TenantViewModel
{
    public IdViewModel Id { get; set; } = IdViewModel.Empty();
    public NameViewModel Name { get; set; } = NameViewModel.Create(string.Empty);
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
