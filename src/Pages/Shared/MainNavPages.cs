using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdvantagePlatform.Pages.Shared
{
    public class MainNavPages
    {
        public static string About => "/Pages/About";
        public static string Index => "/Pages/Index";
        public static string ResourceLinks => "/Pages/ResourceLinks";
        public static string Tools => "/Pages/Tools";

        public static string AboutNavClass(ViewContext viewContext) => PageNavClass(viewContext, About);
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
        public static string ResourceLinksNavClass(ViewContext viewContext) => PageNavClass(viewContext, ResourceLinks);
        public static string ToolsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Tools);

        public static string PageNavClass(ViewContext viewContext, string path)
        {
            var activePath = viewContext.View.Path;
            return activePath.StartsWith(path, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
