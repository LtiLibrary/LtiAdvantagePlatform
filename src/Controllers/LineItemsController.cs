using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Identity.UI.Pages.Internal.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    public class LineItemsController : LineItemsControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LineItemsController(
            ILogger<LineItemsControllerBase> logger, 
            ApplicationDbContext context) : base(logger)
        {
            _context = context;
        }

        protected override async Task<LineItemResult> OnCreateLineItemAsync(PostLineItemRequest request)
        {
            var lineItem = request.LineItem;
            lineItem.Id = lineItem.GetHashCode().ToString();
            return Created(lineItem);
        }

        protected override async Task<IActionResult> OnDeleteLineItemAsync(DeleteLineItemRequest request)
        {
            return Ok();
        }

        protected override async Task<LineItemResult> OnGetLineItemAsync(GetLineItemRequest request)
        {
            var lineItem = new LineItem();
            lineItem.Id = lineItem.GetHashCode().ToString();
            return Ok(lineItem);
        }

        protected override async Task<IActionResult> OnUpdateLineItemAsync(PutLineItemRequest request)
        {
            return Ok();
        }

        protected override async Task<LineItemContainerResult> OnGetLineItemsAsync(GetLineItemsRequest request)
        {
            var course = await _context.Courses.FindAsync(request.ContextId);
            if (course == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.ResourceLinks)
                .Include(u => u.Tools)
                .SingleOrDefaultAsync(u => u.Id == course.UserId);
            if (user == null)
            {
                return NotFound();
            }
            
            // This simulates the declaritive/couple relationship between a line item and a resource link.
            var lineitems = new LineItemContainer();
            var resourceLinks = user.ResourceLinks
                .Where(l => l.LinkContext == ResourceLink.LinkContexts.Course)
                .ToList();

            foreach (var resourceLink in resourceLinks)
            {
                var lineitem = new LineItem
                {
                    LtiLinkId = resourceLink.Id.ToString(),
                    ScoreMaximum = 100
                };
                lineitem.Id = lineitem.GetHashCode().ToString();
                lineitems.Add(lineitem);
            }

            return Ok(lineitems);
        }
    }
}
