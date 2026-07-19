using System.Net;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.IdentificationTypes;
using OroIdentityServer.Application.Modules.IdentificationTypes.Commands;
using OroIdentityServer.Application.Modules.IdentificationTypes.DTOs;
using OroIdentityServer.Application.Modules.IdentificationTypes.Queries;

namespace IdentityServer.Services;

public class ServerAdminIdentificationTypeService(
    IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : IAdminIdentificationTypeService
{
    public async Task<ApiResponse<IEnumerable<IdentificationTypeModel>>?> GetIdentificationTypesAsync(CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetIdentificationTypesQuery(), ct);
        return new ApiResponse<IEnumerable<IdentificationTypeModel>>
        {
            Data = result.Data?.Select(MapIdentificationType).ToList() ?? [],
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<ApiResponse<IdentificationTypeModel>?> GetIdentificationTypeByIdAsync(Guid id, CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetIdentificationTypeByIdQuery(id), ct);
        return new ApiResponse<IdentificationTypeModel>
        {
            Data = result.Data is null ? null : MapIdentificationType(result.Data),
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<HttpResponseMessage> CreateIdentificationTypeAsync(CreateIdentificationTypeRequest request, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new CreateIdentificationTypeCommand(request.Name), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdateIdentificationTypeAsync(Guid id, UpdateIdentificationTypeRequest request, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new UpdateIdentificationTypeCommand(id, request.Name), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> DeleteIdentificationTypeAsync(Guid id, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new DeleteIdentificationTypeCommand(id), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    private static IdentificationTypeModel MapIdentificationType(IdentificationTypeDto dto) => new(dto.Id, dto.Name, dto.IsActive, dto.CreatedAtUtc);
}
