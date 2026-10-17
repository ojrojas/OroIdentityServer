// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

/// <summary>
/// Everything needed to show (or re-show) the enrolment step: the provisioning URI the QR
/// encodes, and the same secret in a form the user can type in by hand.
/// </summary>
public sealed record MfaEnrollmentPayload(string ProvisioningUri, string Secret, DateTime ExpiresAtUtc);