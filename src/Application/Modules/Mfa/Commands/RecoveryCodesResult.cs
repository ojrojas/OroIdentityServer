// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

/// <summary>Recovery codes are returned once, in plaintext, and never again.</summary>
public sealed record RecoveryCodesResult(IReadOnlyList<string> RecoveryCodes);