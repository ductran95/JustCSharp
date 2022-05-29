using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JustCSharp.Api.Swashbuckle
{
    public class ApiKeyResponseOperationFilter : IOperationFilter
    {
        private readonly string _apiKeyName;
        private readonly string _apiKeyRealm;

        public ApiKeyResponseOperationFilter(string apiKeyRealm, string apiKeyName)
        {
            _apiKeyRealm = apiKeyRealm;
            _apiKeyName = apiKeyName;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check for authorize attribute
            var hasAuthorize =
                context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (!hasAuthorize) return;

            operation.Responses.TryAdd("401", new OpenApiResponse {Description = "Unauthenticated"});
            operation.Responses.TryAdd("403", new OpenApiResponse {Description = "Unauthorized"});

            var authScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "ApiKey"}
            };

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [authScheme] = new string[] { }
                }
            };
        }
    }
}
