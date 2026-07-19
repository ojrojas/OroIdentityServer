namespace IdentityServer.Client.Models.IdentificationTypes;

public sealed record IdentificationTypeModel(Guid Id, string Name, bool IsActive, DateTime CreatedAtUtc);

public sealed record CreateIdentificationTypeRequest(string Name);

public sealed record UpdateIdentificationTypeRequest(string Name);
