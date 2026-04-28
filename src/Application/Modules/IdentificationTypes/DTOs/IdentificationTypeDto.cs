namespace OroIdentityServer.Application.Modules.IdentificationTypes.DTOs;

public sealed record IdentificationTypeDto(
    Guid Id, 
    string Name, 
    bool IsActive, 
    DateTime CreatedAtUtc);
