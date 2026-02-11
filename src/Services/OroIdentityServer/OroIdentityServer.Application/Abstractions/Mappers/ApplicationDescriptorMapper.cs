using System.Globalization;
using System.Text.Json;
using OpenIddict.Abstractions;
using OroIdentityServer.Services.OroIdentityServer.Application.Commands;

namespace OroIdentityServer.Application.Abstractions.Mappers;

public static class ApplicationDescriptorMapper
{
	public static OpenIddictApplicationDescriptor ToOpenIddict(this ApplicationDescriptor? src)
	{
		if (src == null) return null!;

		var dest = new OpenIddictApplicationDescriptor
		{
			ApplicationType = src.ApplicationType,
			ClientId = src.ClientId,
			ClientSecret = src.ClientSecret,
			ClientType = src.ClientType,
			ConsentType = src.ConsentType,
			DisplayName = src.DisplayName,
			JsonWebKeySet = src.JsonWebKeySet
		};

		if (src.DisplayNames != null)
		{
			foreach (var kv in src.DisplayNames)
			{
				dest.DisplayNames[kv.Key] = kv.Value;
			}
		}

		if (src.Permissions != null)
		{
			foreach (var permission in src.Permissions)
				dest.Permissions.Add(permission);
		}

		if (src.Requirements != null)
		{
			foreach (var req in src.Requirements)
				dest.Requirements.Add(req);
		}

		if (src.RedirectUris != null)
		{
			foreach (var uri in src.RedirectUris)
				dest.RedirectUris.Add(uri);
		}

		if (src.PostLogoutRedirectUris != null)
		{
			foreach (var uri in src.PostLogoutRedirectUris)
				dest.PostLogoutRedirectUris.Add(uri);
		}

		if (src.Properties != null)
		{
			foreach (var kv in src.Properties)
				dest.Properties[kv.Key] = kv.Value;
		}

		if (src.Settings != null)
		{
			foreach (var kv in src.Settings)
				dest.Settings[kv.Key] = kv.Value;
		}

		return dest;
	}

	public static ApplicationDescriptor ToApplicationDescriptor(this OpenIddictApplicationDescriptor? src)
	{
		if (src == null) return null!;

		var dest = new ApplicationDescriptor
		{
			ApplicationType = src.ApplicationType,
			ClientId = src.ClientId,
			ClientSecret = src.ClientSecret,
			ClientType = src.ClientType,
			ConsentType = src.ConsentType,
			DisplayName = src.DisplayName,
			JsonWebKeySet = src.JsonWebKeySet
		};

		if (src.DisplayNames != null)
		{
			foreach (var kv in src.DisplayNames)
				dest.DisplayNames[kv.Key] = kv.Value;
		}

		if (src.Permissions != null)
		{
			foreach (var permission in src.Permissions)
				dest.Permissions.Add(permission);
		}

		if (src.Requirements != null)
		{
			foreach (var req in src.Requirements)
				dest.Requirements.Add(req);
		}

		if (src.RedirectUris != null)
		{
			foreach (var uri in src.RedirectUris)
				dest.RedirectUris.Add(uri);
		}

		if (src.PostLogoutRedirectUris != null)
		{
			foreach (var uri in src.PostLogoutRedirectUris)
				dest.PostLogoutRedirectUris.Add(uri);
		}

		if (src.Properties != null)
		{
			foreach (var kv in src.Properties)
				dest.Properties[kv.Key] = kv.Value;
		}

		if (src.Settings != null)
		{
			foreach (var kv in src.Settings)
				dest.Settings[kv.Key] = kv.Value;
		}

		return dest;
	}
}