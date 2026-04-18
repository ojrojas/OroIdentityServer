// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Server.Helpers;

// Helper: stores discovered server ports and exposes readiness
public sealed class ServerPortProvider
{
    private readonly List<int> _ports = [];
    private readonly TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IReadOnlyList<int> Ports => _ports.AsReadOnly();
    public bool IsReady => _ready.Task.IsCompleted;
    public Task WaitForReadyAsync() => _ready.Task;

    internal void SetPorts(IEnumerable<int> ports)
    {
        _ports.Clear();
        _ports.AddRange(ports);
        _ready.TrySetResult(true);
    }
}