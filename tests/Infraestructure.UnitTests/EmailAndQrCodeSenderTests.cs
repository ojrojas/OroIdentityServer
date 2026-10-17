// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using Microsoft.Extensions.Options;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Notifications.Options;
using OroIdentityServer.Infraestructure.Services;

public class FileSystemEmailSenderTests : IDisposable
{
    private readonly string _outbox = Path.Combine(Path.GetTempPath(), "oro-emails-" + Guid.NewGuid().ToString("N"));

    private FileSystemEmailSender CreateSender(bool enabled = true)
    {
        var options = Options.Create(new EmailOptions
        {
            Enabled = enabled,
            Provider = "File",
            OutboxPath = _outbox,
            FromAddress = "no-reply@oroidentity.local",
            FromName = "OroIdentity"
        });

        return new FileSystemEmailSender(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<FileSystemEmailSender>.Instance,
            options);
    }

    [Fact]
    public async Task SendAsync_WritesTheBodyAndAnEnvelopeSidecar()
    {
        var sender = CreateSender();

        await sender.SendAsync(new EmailMessage(
            "user@example.com",
            "Your session expired",
            "<p>body</p>",
            EmailBodyFormat.Html,
            TemplateName: "SessionExpired"));

        var body = Assert.Single(Directory.GetFiles(_outbox, "*.html"));
        Assert.Equal("<p>body</p>", await File.ReadAllTextAsync(body));

        var envelope = Assert.Single(Directory.GetFiles(_outbox, "*.json"));
        var json = await File.ReadAllTextAsync(envelope);
        Assert.Contains("user@example.com", json);
        Assert.Contains("Your session expired", json);
        Assert.Contains("SessionExpired", json);
        Assert.Contains("no-reply@oroidentity.local", json);
    }

    [Fact]
    public async Task SendAsync_UsesATextExtensionForPlainTextBodies()
    {
        var sender = CreateSender();

        await sender.SendAsync(new EmailMessage("user@example.com", "Subject", "plain", EmailBodyFormat.Text));

        Assert.Single(Directory.GetFiles(_outbox, "*.txt"));
        Assert.Empty(Directory.GetFiles(_outbox, "*.html"));
    }

    [Fact]
    public async Task SendAsync_DropsTheMessageWhenDeliveryIsDisabled()
    {
        var sender = CreateSender(enabled: false);

        await sender.SendAsync(new EmailMessage("user@example.com", "Subject", "<p>body</p>"));

        Assert.False(Directory.Exists(_outbox));
    }

    [Fact]
    public async Task SendAsync_SanitisesTheRecipientInTheFileName()
    {
        var sender = CreateSender();

        await sender.SendAsync(new EmailMessage("../../etc/passwd", "Subject", "<p>body</p>"));

        // The name must stay inside the outbox.
        Assert.NotEmpty(Directory.GetFiles(_outbox));
        foreach (var file in Directory.GetFiles(_outbox))
            Assert.StartsWith(Path.GetFullPath(_outbox), Path.GetFullPath(file));
    }

    [Fact]
    public async Task SendAsync_KeepsMessagesSeparate()
    {
        var sender = CreateSender();

        await sender.SendAsync(new EmailMessage("a@example.com", "One", "<p>1</p>"));
        await sender.SendAsync(new EmailMessage("a@example.com", "Two", "<p>2</p>"));

        Assert.Equal(2, Directory.GetFiles(_outbox, "*.html").Length);
        Assert.Equal(2, Directory.GetFiles(_outbox, "*.json").Length);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outbox)) Directory.Delete(_outbox, recursive: true);
        GC.SuppressFinalize(this);
    }
}

public class QrCodeGeneratorTests
{
    [Fact]
    public void GeneratePng_ReturnsARealPngImage()
    {
        IQrCodeGenerator generator = new QrCodeGenerator();

        var png = generator.GeneratePng("otpauth://totp/OroIdentity:user?secret=JBSWY3DPEHPK3PXP");

        Assert.NotEmpty(png);
        // PNG magic number.
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, png.Take(4));
    }

    [Fact]
    public void GeneratePng_GrowsWithTheModuleSize()
    {
        IQrCodeGenerator generator = new QrCodeGenerator();
        const string payload = "otpauth://totp/OroIdentity:user?secret=JBSWY3DPEHPK3PXP";

        var small = generator.GeneratePng(payload, 4);
        var large = generator.GeneratePng(payload, 12);

        Assert.True(large.Length > small.Length);
    }

    [Fact]
    public void GeneratePng_RejectsAnEmptyPayload()
    {
        IQrCodeGenerator generator = new QrCodeGenerator();

        Assert.Throws<ArgumentException>(() => generator.GeneratePng("  "));
    }
}