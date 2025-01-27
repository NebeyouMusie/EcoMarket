using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace EcoMarket.Swagger
{
    public class SwaggerAuthorizationOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the endpoint has [AllowAnonymous]
            var allowAnonymous = context.MethodInfo.GetCustomAttributes<AllowAnonymousAttribute>().Any();
            var hasAuthorize = context.MethodInfo.GetCustomAttributes<AuthorizeAttribute>().Any() ||
                              context.MethodInfo.DeclaringType?.GetCustomAttributes<AuthorizeAttribute>().Any() == true;

            if (hasAuthorize && !allowAnonymous)
            {
                // Add security requirement for endpoints that require authorization
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    }
                };
            }
            else
            {
                // Remove security requirement for endpoints with [AllowAnonymous]
                operation.Security = new List<OpenApiSecurityRequirement>();
            }
        }
    }
}
