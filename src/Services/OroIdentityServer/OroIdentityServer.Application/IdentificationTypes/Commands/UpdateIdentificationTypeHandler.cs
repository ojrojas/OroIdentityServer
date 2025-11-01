// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class UpdateIdentificationTypeHandler(
    ILogger<UpdateIdentificationTypeHandler> logger,
    IIdentificationTypeRepository repository
) : ICommandHandler<UpdateIdentificationTypeCommand>
{
    public Task HandleAsync(UpdateIdentificationTypeCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}