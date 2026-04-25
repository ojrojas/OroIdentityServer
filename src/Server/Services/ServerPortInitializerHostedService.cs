// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Server.Helpers;

namespace OroIdentityServer.Server.Services;

// Hosted service: runs on startup, inspects the server (features or via reflection) and fills the provider
public sealed class ServerPortInitializerHostedService(IServiceProvider services, ILogger<ServerPortInitializerHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // ensure we don't block startup sync contexts
            await Task.Yield();

            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var ports = new List<int>();

            // Try to get IServer and then IServerAddressesFeature
            if (provider.GetService(typeof(Microsoft.AspNetCore.Hosting.Server.IServer)) is Microsoft.AspNetCore.Hosting.Server.IServer server)
            {
                var featuresProp = server.GetType().GetProperty("Features", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var features = featuresProp?.GetValue(server) as Microsoft.AspNetCore.Http.Features.IFeatureCollection;
                var addressesFeature = features?.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
                if (addressesFeature != null)
                {
                    foreach (var address in addressesFeature.Addresses)
                    {
                        if (Uri.TryCreate(address, UriKind.Absolute, out var uri) && uri.Port > 0)
                            ports.Add(uri.Port);
                        else
                        {
                            var idx = address.LastIndexOf(':');
                            if (idx > -1 && int.TryParse(address.Substring(idx + 1), out var p))
                                ports.Add(p);
                        }
                    }
                }

                // Reflection fallback into Kestrel internals
                if (ports.Count == 0 && server.GetType().FullName?.Contains("KestrelServer") == true)
                {
                    object kestrelOptions = null;
                    var optionsField = server.GetType().GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (optionsField != null)
                        kestrelOptions = optionsField.GetValue(server);
                    else
                    {
                        var optProp = server.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .FirstOrDefault(p => p.PropertyType.Name.Contains("KestrelServerOptions"));
                        if (optProp != null)
                            kestrelOptions = optProp.GetValue(server);
                    }

                    if (kestrelOptions != null)
                    {
                        var listenProp = kestrelOptions.GetType().GetProperty("ListenOptions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var listenOpts = listenProp?.GetValue(kestrelOptions) as IEnumerable;
                        if (listenOpts != null)
                        {
                            foreach (var lo in listenOpts)
                            {
                                var epProp = lo.GetType().GetProperty("EndPoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                var ep = epProp?.GetValue(lo) as System.Net.EndPoint;
                                if (ep is System.Net.IPEndPoint ipe)
                                {
                                    ports.Add(ipe.Port);
                                }
                                else
                                {
                                    var portProp = lo.GetType().GetProperty("Port", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (portProp != null && portProp.GetValue(lo) is int p)
                                        ports.Add(p);
                                }
                            }
                        }
                    }
                }
            }

            // Config fallback (ASPNETCORE_URLS / urls)
            if (ports.Count == 0)
            {
                var config = provider.GetService(typeof(IConfiguration)) as IConfiguration;
                var urls = config? ["ASPNETCORE_URLS"] ?? config?["urls"] ?? config?["Urls"];
                if (!string.IsNullOrEmpty(urls))
                {
                    foreach (var part in urls.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (Uri.TryCreate(part, UriKind.Absolute, out var uri2) && uri2.Port > 0)
                            ports.Add(uri2.Port);
                        else
                        {
                            var idx = part.LastIndexOf(':');
                            if (idx > -1 && int.TryParse(part.Substring(idx + 1), out var p))
                                ports.Add(p);
                        }
                    }
                }
            }

            var providerSingleton = provider.GetService(typeof(ServerPortProvider)) as ServerPortProvider;
            providerSingleton?.SetPorts([.. ports.Distinct()]);
            logger.LogInformation("Server ports discovered: {ports}", string.Join(", ", ports.Distinct()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not determine server ports via reflection.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}