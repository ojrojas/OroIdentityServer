// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Extensions;

/// <summary>
/// Provides extension methods for configuring services in the application.
/// </summary>
public static class CoreExtensions
{
    public static TBuilder AddCoreExtensions<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
        return builder;
    }

}