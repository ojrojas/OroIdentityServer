// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Services;

using QRCoder;

/// <summary>
/// Renders the enrolment payload as a PNG. Server-side on purpose: the authentication pages are
/// statically rendered (excluded from interactive routing), so they cannot run JavaScript to
/// draw the QR themselves.
/// </summary>
public sealed class QrCodeGenerator : IQrCodeGenerator
{
    public byte[] GeneratePng(string payload, int pixelsPerModule = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var png = new PngByteQRCode(data);

        return png.GetGraphic(pixelsPerModule);
    }
}