namespace IdentityServer.Client.Services;

public sealed class TenantHeaderHandler(ICurrentTenantContext tenantContext) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tenantContext.CurrentTenantId is { } tenantId && !request.Headers.Contains("X-Tenant-Id"))
            request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId.ToString());

        return base.SendAsync(request, cancellationToken);
    }
}
