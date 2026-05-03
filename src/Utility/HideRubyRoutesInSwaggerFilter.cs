using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AdvantagePlatform.Utility
{
    /// <summary>
    /// Removes routes from the OpenAPI document that include ".{format}" which is used
    /// to match endpoints for Ruby clients that specify formatting (e.g. ".json").
    /// </summary>
    public class HideRubyRoutesInSwaggerFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var key in swaggerDoc.Paths.Keys.ToArray())
            {
                if (key.Contains(".{format}"))
                {
                    swaggerDoc.Paths.Remove(key);
                }
            }
        }
    }
}
