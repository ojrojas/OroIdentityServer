// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Interfaces;

/// <summary>
/// Renders a payload as a QR code image, used for the authenticator enrolment secret.
/// </summary>
public interface IQrCodeGenerator
{
    /// <summary>PNG bytes for the given text payload.</summary>
    byte[] GeneratePng(string payload, int pixelsPerModule = 10);
}