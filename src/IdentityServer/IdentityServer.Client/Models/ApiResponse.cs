using System.Text.Json;

namespace IdentityServer.Client.Models;

public sealed class ApiResponse<T>
{
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];
}

public static class ClientJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
}
