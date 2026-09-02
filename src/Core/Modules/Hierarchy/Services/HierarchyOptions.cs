// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
namespace OroIdentityServer.Core.Modules.Hierarchy.Services;

public sealed class HierarchyOptions
{
    public const string SectionName = "Hierarchy";

    /// <summary>
    /// Maximum depth for recursive traversal (default 10)
    /// </summary>
    public int MaxDepth { get; set; } = 10;

    /// <summary>
    /// Maximum subordinates per user
    /// </summary>
    public int MaxSubordinatesPerUser { get; set; } = 1000;

    /// <summary>
    /// Maximum superiors per user
    /// </summary>
    public int MaxSuperiorsPerUser { get; set; } = 5;
}
