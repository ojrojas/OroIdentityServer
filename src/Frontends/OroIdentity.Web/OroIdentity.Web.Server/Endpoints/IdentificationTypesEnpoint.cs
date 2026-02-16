using Microsoft.AspNetCore.Mvc;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;
using OroIdentity.Web.Server.Services;

namespace OroIdentity.Web.Server.Endpoints;

public static class IdentificationTypesEndpoints
{
    public static RouteGroupBuilder MapIdentificationTypesEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api/v1/identificationtypes")
            .WithTags("Identification Types");

        group.MapGet(string.Empty, GetAllIdentificationTypes);
        group.MapGet("{id:guid}", GetIdentificationTypeById);
        group.MapPost(string.Empty, CreateIdentificationType);
        group.MapPut(string.Empty, UpdateIdentificationType);
        group.MapDelete("{id:guid}", DeleteIdentificationType);

        return group;
    }

    private static async Task DeleteIdentificationType(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task UpdateIdentificationType(
        HttpContext context,
        [FromBody] IdentificationTypeViewModel identificationType,
        [FromServices] IIdentificationTypeService service,
        CancellationToken cancellationToken)
    {
        await service.UpdateIdentificationTypeAsync(identificationType, cancellationToken);
    }

    private static async Task CreateIdentificationType(
        HttpContext context,
        [FromBody] IdentificationTypeViewModel identificationType,
        [FromServices] IIdentificationTypeService service,
        CancellationToken cancellationToken
        )
    {
        await service.CreateIdentificationTypeAsync(identificationType, cancellationToken);
    }

    private static async Task<BaseResponseViewModel<IdentificationTypeViewModel>>  
    GetIdentificationTypeById(
        HttpContext context,
        [FromRoute] Guid id,
        [FromServices] IIdentificationTypeService service,
        CancellationToken cancellationToken)
        
    {
        return await service.GetIdentificationTypeByIdAsync(id.ToString(), cancellationToken);
    }

    private static async Task<BaseResponseViewModel<IEnumerable<IdentificationTypeViewModel>>> 
    GetAllIdentificationTypes(
        HttpContext context,
        [FromServices] IIdentificationTypeService service,
        CancellationToken cancellationToken)
    {
        return await service.GetAllIdentificationTypesAsync(cancellationToken);
    }
}