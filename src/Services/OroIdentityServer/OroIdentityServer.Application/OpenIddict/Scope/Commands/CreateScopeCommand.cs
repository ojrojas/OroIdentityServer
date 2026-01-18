// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public record CreateScopeCommand(string Name, IEnumerable<string> Resources) : ICommand
{
    public Guid CorrelationId()=> Guid.NewGuid();
}