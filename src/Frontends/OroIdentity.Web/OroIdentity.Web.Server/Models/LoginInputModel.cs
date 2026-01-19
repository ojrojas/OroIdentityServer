// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.ComponentModel.DataAnnotations;

namespace OroIdentity.Web.Server.Models;

public sealed class LoginInputModel
{
    // [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}