// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentity.Web.Client.Models;

public class ScopeViewModel
{
    /// <summary>
    /// Id scope 
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Name scope 
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Description scope
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Resources associated to scope
    /// </summary>
    public IEnumerable<string> Resources { get; set; } = [];
}