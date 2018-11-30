using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdvantagePlatform.Pages.Shared
{
    public class MainNavPages
    {
        public static string About => "/Pages/About";
        public static string CourseLinks => "/Pages/CourseLinks";
        public static string Index => "/Pages/Index";
        public static string PlatformLinks => "/Pages/PlatformLinks";
        public static string Tools => "/Pages/Tools";

        public static string AboutNavClass(ViewContext viewContext) => PageNavClass(viewContext, About);
        public static string CourseLinksNavClass(ViewContext viewContext) => PageNavClass(viewContext, CourseLinks);
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
        public static string PlatformLinksNavClass(ViewContext viewContext) => PageNavClass(viewContext, PlatformLinks);
        public static string ToolsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Tools);

        public static string PageNavClass(ViewContext viewContext, string path)
        {
            var activePath = viewContext.View.Path;
            return activePath.StartsWith(path, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
