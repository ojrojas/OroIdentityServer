using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BlazorServerSessionExample.Services;

/// <summary>
/// Monitors session validity by periodically introspecting the access token
/// against the Identity Server's introspection endpoint.
/// </summary>
public sealed class SessionMonitor
{
    private readonly HttpClient _http;
    private readonly string _introspectUrl;
    private readonly ILogger<SessionMonitor> _logger;

    public SessionMonitor(HttpClient http, IConfiguration config, ILogger<SessionMonitor> logger)
    {
        _http = http;
        _logger = logger;
        _introspectUrl = $"{config["IdentityServer:Authority"]}/connect/introspect";
    }

    /// <summary>
    /// Checks if the given access token is still active (not revoked, not expired).
    /// </summary>
    public async Task<bool> IsTokenActiveAsync(string accessToken, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsync(_introspectUrl,
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["token"] = accessToken,
                    ["token_type_hint"] = "access_token"
                }), ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IntrospectionResponse>(cancellationToken: ct);
            return result?.Active == true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to introspect token, treating as inactive");
            return false;
        }
    }
}

public sealed class IntrospectionResponse
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("sub")]
    public string? Subject { get; set; }

    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    [JsonPropertyName("exp")]
    public long? ExpiresAt { get; set; }

    [JsonPropertyName("iat")]
    public long? IssuedAt { get; set; }
}
