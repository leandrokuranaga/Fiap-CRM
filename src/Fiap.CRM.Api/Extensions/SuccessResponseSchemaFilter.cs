using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fiap.CRM.Api.Extensions;

[ExcludeFromCodeCoverage]
public class SuccessResponseSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsGenericType &&
            context.Type.GetGenericTypeDefinition().Name.StartsWith("SuccessResponse"))
        {
            if (schema.Properties.TryGetValue("error", out var errorSchema))
            {
                errorSchema.Example = new OpenApiNull();
                errorSchema.Nullable = true;
            }
        }
    }
}
