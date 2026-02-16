namespace OroIdentity.Web.Client.Models;

public record CreateApplicationRequest(string ClientId, string DisplayName, string ApplicationType, string ClientType, string ConsentType);

public record UpdateApplicationRequest(string ClientId, string DisplayName, string ApplicationType, string ClientType, string ConsentType);

public record GetApplicationsResponse(IEnumerable<ApplicationViewModel>? Data = null);

public record GetApplicationResponse(ApplicationViewModel? Data = null);

public record CreateApplicationResponse(Guid? Id = null);

public record UpdateApplicationResponse;