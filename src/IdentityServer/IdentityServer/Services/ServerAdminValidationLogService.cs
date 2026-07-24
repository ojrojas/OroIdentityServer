using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models.Diagnostics;
using OroIdentityServer.Application.Modules.Diagnostics.Queries;

namespace IdentityServer.Services;

internal class ServerAdminValidationLogService(
    IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher
) : IAdminValidationLogService
{
    public async Task<IReadOnlyList<DailyValidationCountModel>> GetDailySummaryAsync(int days = 7, CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetValidationLogDailySummaryQuery(days), ct);
         List<DailyValidationCountModel> response = [];
        if(result?.Days.Count != 0)
        {
           response = [.. result.Days.Select(d => new DailyValidationCountModel(d.Date, d.SucceededCount, d.FailedCount))];
           return response;
        }
       
       return [];
    }
}