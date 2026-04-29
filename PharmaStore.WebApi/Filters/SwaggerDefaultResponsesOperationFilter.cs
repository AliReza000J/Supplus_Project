using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerDefaultResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add 401/403 responses to operations that have [Authorize]
        var hasAuth = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                      || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (hasAuth)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        }

        // Optionally: add examples or more detailed response schema via context.SchemaGenerator
    }
}
