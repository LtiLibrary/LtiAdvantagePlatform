using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }
        public int? HttpStatusCode { get; set;  }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet(int? httpStatusCode)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            HttpStatusCode = httpStatusCode;
        }
    }
}
