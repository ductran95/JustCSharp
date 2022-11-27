using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace JustCSharp.AspNetCore.Extensions;

public static class HttpContextExtensions
{
    public static bool HasJsonContentType(this HttpResponse response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (!MediaTypeHeaderValue.TryParse(response.ContentType, out var mt))
        {
            return false;
        }

        // Matches application/json
        if (mt.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Matches +json, e.g. application/ld+json
        if (mt.Suffix.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
    
    public static bool IsApiRequest(this HttpRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return request.Path.StartsWithSegments("/api")
               || request.Path.StartsWithSegments("/healthcheck")
               || request.Path.StartsWithSegments("/graphql");
    }
}