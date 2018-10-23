using System.Collections.Generic;
using System.Linq;
using IdentityServer4.EntityFramework.Entities;

namespace AdvantagePlatform.Pages.Clients
{
    public static class ClientPropertiesExtensions
    {
        public static string GetValue(this IList<ClientProperty> properties, string key)
        {
            return properties.FirstOrDefault(p => p.Key == key)?.Value;
        }
    }
}
