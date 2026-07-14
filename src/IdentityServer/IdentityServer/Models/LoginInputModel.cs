namespace IdentityServer.Server.Models;

public record LoginInputModel
{
    public required string LoginIdentifier { get; set; }
    public required string Password { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
}