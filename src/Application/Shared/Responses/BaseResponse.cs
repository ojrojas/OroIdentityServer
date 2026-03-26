// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Shared;

public abstract record BaseResponse<T> 
{
    public T? Data { get; set; }
    public int StatusCode { get; set; } = 200;
    public string Message { get; set; } = "Response Ok";
    public List<string> Errors { get; set; } = [];
}
