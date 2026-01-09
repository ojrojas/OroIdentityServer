// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Enums;

public enum ResultTypes
{
    SignIn,
    SignOut,
    Forbid,
    BadRequest,
    Challenge, 
    OK,
    Unauthorized, 
    Redirect
}