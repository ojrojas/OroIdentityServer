// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.ComponentModel.DataAnnotations;

namespace OroIdentity.Web.Client.Models;

public class UserViewModel
{
    /// <summary>
    /// Name user
    /// </summary>
    [Required]
    public string Name { get; set; }
    public string MiddleName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Identification { get; set; }
    [Required]
    public Guid IdentificationTypeId { get; set; }

    public SecurityUserViewModel Security { get; set; } = new SecurityUserViewModel();
}

public class SecurityUserViewModel
{
    public string? PasswordHash { get; set; }
    public string? SecurityStamp { get; set; } = string.Empty;
    public string? ConcurrencyStamp { get; set; }
    public DateTime? LockoutEnd { get; set; } = DateTime.UtcNow;
    public bool LockoutEnabled { get; set; } = false;
    public int AccessFailedCount { get; set; } = 0;
}