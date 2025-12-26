// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Interfaces;

/// <summary>
/// Specification pattern interface
/// </summary>
/// <typeparam name="T">The type of the entity to which the specification applies.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the criteria expression that defines the specification.
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }
}