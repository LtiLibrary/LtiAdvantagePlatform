using System;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AdvantagePlatform.Utility
{
    public class HideRubyRoutesInSwaggerFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var key in swaggerDoc.Paths.Keys.ToArray())
            {
                if (key.EndsWith(".{format}"))
                {
                    swaggerDoc.Paths.Remove(key);
                }
            }
        }
    }
}
