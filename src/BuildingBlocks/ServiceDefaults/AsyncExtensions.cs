// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.ServiceDefaults;

public static class AsyncExtensions
{
    /// <summary>
    /// To list async scope inner into awaiter
    /// </summary>
    /// <typeparam name="T">Type parameter list</typeparam>
    /// <param name="source">Source elements into list</param>
    /// <returns>List sync</returns>
    /// <exception cref="ArgumentNullException">Argument null exception</exception>
    public static ValueTask<List<T>> ToListExtensionsAsync<T>(this IAsyncEnumerable<T> source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return ExecutionAsync();

        async ValueTask<List<T>> ExecutionAsync()
        {
            var list = new List<T>();

            await foreach (var i in source)
            {
                list.Add(i);
            }

            return list;
        }
    }
}