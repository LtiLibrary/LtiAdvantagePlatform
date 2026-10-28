using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace AdvantagePlatform.Utility
{
    /// <summary>
    /// Paging for the NRPS membership and AGS line item containers (NRPS 2.0 §2.4.2,
    /// AGS 2.0 §3.2.5): when the tool sends <c>limit</c>, return one page and point to the next
    /// one with a <c>Link: &lt;url&gt;; rel="next"</c> header. The page number travels in the
    /// next URL's <c>page</c> parameter, next to the tool's own filters.
    /// </summary>
    public static class ServicePaging
    {
        public static List<T> Page<T>(HttpContext httpContext, IEnumerable<T> items, int? limit)
        {
            var all = items.ToList();
            if (limit is not > 0)
            {
                return all;
            }

            var request = httpContext.Request;
            var page = int.TryParse(request.Query["page"], out var p) && p > 0 ? p : 1;
            var pageItems = all.Skip((page - 1) * limit.Value).Take(limit.Value).ToList();

            if (page * limit.Value < all.Count)
            {
                var query = QueryHelpers.ParseQuery(request.QueryString.Value);
                query["page"] = (page + 1).ToString();
                var next = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path,
                    QueryString.Create(query));
                httpContext.Response.Headers.Append("Link", $"<{next}>; rel=\"next\"");
            }

            return pageItems;
        }
    }
}
