using System.Diagnostics;
using JustCSharp.AspNetCore.Extensions;
using JustCSharp.AspNetCore.OpenTelemetry.Buffering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace JustCSharp.AspNetCore.OpenTelemetry.Middlewares;

public class OpenTelemetryApiBodyLoggingMiddleware
{
    private readonly ILogger<OpenTelemetryApiBodyLoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public OpenTelemetryApiBodyLoggingMiddleware(
        RequestDelegate next,
        ILogger<OpenTelemetryApiBodyLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogTrace("Start logging request/response body");
        DateTime startTime = DateTime.UtcNow;
        DateTime? beforeNext = null;
        DateTime? afterNext = null;
            
        RequestBufferingStream? bufferedRequestStream = null;
        Stream? originalRequestStream = null;
        
        ResponseBufferingStream? bufferedResponseStream = null;
        IHttpResponseBodyFeature? originalBodyFeature = null;

        try
        {
            if (context.Request.Body.CanRead && context.Request.HasJsonContentType())
            {
                originalRequestStream = context.Request.Body;
                bufferedRequestStream = new RequestBufferingStream(originalRequestStream);
                context.Request.Body = bufferedRequestStream;
            }

            originalBodyFeature = context.Features.Get<IHttpResponseBodyFeature>()!;
            bufferedResponseStream = new ResponseBufferingStream(originalBodyFeature);
            context.Response.Body = bufferedResponseStream;
            context.Features.Set<IHttpResponseBodyFeature>(bufferedResponseStream);
            
            beforeNext = DateTime.UtcNow;
            await _next(context);
            afterNext = DateTime.UtcNow;
            
            var requestBody = bufferedRequestStream?.ReadText();
            Activity.Current?.SetTag(SemanticConventions.AttributeHttpRequestBody, requestBody);

            if (context.Response.Body.CanWrite && context.Response.HasJsonContentType())
            {
                var responseBody = bufferedResponseStream.ReadText();
                Activity.Current?.SetTag(SemanticConventions.AttributeHttpResponseBody, responseBody);
            }
        }
        finally
        {
            bufferedRequestStream?.Dispose();

            if (originalRequestStream != null)
            {
                context.Request.Body = originalRequestStream;
            }
            
            bufferedResponseStream?.Dispose();

            if (originalBodyFeature != null)
            {
                context.Features.Set(originalBodyFeature);
            }
            
            DateTime endTime = DateTime.UtcNow;
            TimeSpan totalTime;
            if (beforeNext != null && afterNext != null)
            {
                totalTime = (beforeNext.Value - startTime) + (endTime - afterNext.Value);
            }
            else
            {
                totalTime = endTime - startTime;
            }

            var tags = new ActivityTagsCollection();
            tags["duration"] = totalTime;
            Activity.Current?.AddEvent(new ActivityEvent("Logging Body", tags: tags));
            
            _logger.LogTrace("End logging request/response body");
        }
    }
}