// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Services;

using System.Text.Json;
using Microsoft.Extensions.Options;
using OroIdentityServer.Core.Modules.Notifications.Options;

/// <summary>
/// Development/test transport: writes the rendered message to disk and logs it, so the flow can
/// be exercised end to end without an SMTP server. Swapping in real delivery later only means
/// registering another <see cref="IEmailSender"/> while <c>Email:Provider</c> says "Smtp".
/// </summary>
public sealed class FileSystemEmailSender(
    ILogger<FileSystemEmailSender> logger,
    IOptions<EmailOptions> options) : IEmailSender
{
    private static readonly JsonSerializerOptions MetadataJsonOptions = new() { WriteIndented = true };

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Email delivery is disabled; dropping message to {To} ({Subject})", message.To, message.Subject);
            return;
        }

        var directory = ResolveOutboxPath(settings);
        Directory.CreateDirectory(directory);

        var stamp = $"{message.CreatedAtUtc:yyyyMMddHHmmssfff}";
        var slug = MakeFileNameSafe(message.To);
        var baseName = $"{stamp}-{slug}-{message.CorrelationId:N}";
        var bodyExtension = message.BodyFormat == EmailBodyFormat.Text ? "txt" : "html";

        await File.WriteAllTextAsync(
            Path.Combine(directory, $"{baseName}.{bodyExtension}"),
            message.Body,
            cancellationToken);

        // A sidecar keeps envelope data (from, template, correlation id) queryable without parsing HTML.
        var envelope = new
        {
            message.To,
            message.Subject,
            From = $"{settings.FromName} <{settings.FromAddress}>",
            Template = message.TemplateName,
            message.CorrelationId,
            QueuedAtUtc = message.CreatedAtUtc
        };

        await File.WriteAllTextAsync(
            Path.Combine(directory, $"{baseName}.json"),
            JsonSerializer.Serialize(envelope, MetadataJsonOptions),
            cancellationToken);

        logger.LogInformation(
            "Queued email '{Subject}' to {To} in {OutboxPath} (correlation {CorrelationId})",
            message.Subject, message.To, directory, message.CorrelationId);
    }

    private static string ResolveOutboxPath(EmailOptions settings)
        => string.IsNullOrWhiteSpace(settings.OutboxPath)
            ? Path.Combine(AppContext.BaseDirectory, "emails")
            : settings.OutboxPath;

    private static string MakeFileNameSafe(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        var result = new string(chars);
        return result.Length <= 40 ? result : result[..40];
    }
}