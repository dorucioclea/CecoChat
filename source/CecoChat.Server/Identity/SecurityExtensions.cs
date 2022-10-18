using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace CecoChat.Server.Identity;

public static class SecurityExtensions
{
    public static bool TryGetBearerAccessTokenValue(this HttpContext context, out string accessToken)
    {
        accessToken = null;
        if (!context.Request.Headers.TryGetValue(HeaderNames.Authorization, out StringValues values))
        {
            return false;
        }
        if (values.Count > 1)
        {
            return false;
        }

        string value = values.First();
        const string bearerPrefix = "Bearer ";
        if (!value.StartsWith(bearerPrefix, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        accessToken = value.Substring(startIndex: bearerPrefix.Length);
        return true;
    }
}