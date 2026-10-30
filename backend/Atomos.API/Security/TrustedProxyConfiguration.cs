using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace Atomos.API.Security;

public static class TrustedProxyConfiguration
{
    public static void Apply(ForwardedHeadersOptions options, IConfiguration configuration)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();

        foreach (var value in configuration.GetSection("Proxy:KnownProxies").Get<string[]>() ?? [])
        {
            if (IPAddress.TryParse(value, out var address)) options.KnownProxies.Add(address);
        }

        foreach (var value in configuration.GetSection("Proxy:KnownNetworks").Get<string[]>() ?? [])
        {
            if (System.Net.IPNetwork.TryParse(value, out var network)) options.KnownIPNetworks.Add(network);
        }
    }
}
