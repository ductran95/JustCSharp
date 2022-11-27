using JustCSharp.AspNetCore.OpenTelemetry.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace JustCSharp.AspNetCore.OpenTelemetry.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseOpenTelemetryBodyLogging(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.UseMiddleware<OpenTelemetryApiBodyLoggingMiddleware>();
    }
}