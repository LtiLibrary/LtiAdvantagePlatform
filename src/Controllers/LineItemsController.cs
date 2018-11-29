using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
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
            var course = await _context.Courses
                .Include(c => c.ResourceLinks)
                .SingleOrDefaultAsync(c => c.Id == request.ContextId);
            if (course == null)
            {
                return NotFound(default(LineItem));
            }
            
            // This assumes a 1:1 relationship between a line item and a resource link.
            var resourceLink = course.ResourceLinks
                .SingleOrDefault(l => l.Id.ToString() == request.Id);
            if (resourceLink == null)
            {
                return NotFound(default(LineItem));
            }

            var lineitem = new LineItem
            {
                ResourceLinkId = resourceLink.Id.ToString(),
                ScoreMaximum = 100.0
            };
            lineitem.Id = lineitem.GetHashCode().ToString();

            return Ok(lineitem);
        }

        protected override async Task<IActionResult> OnUpdateLineItemAsync(PutLineItemRequest request)
        {
            return Ok();
        }

        protected override async Task<LineItemContainerResult> OnGetLineItemsAsync(GetLineItemsRequest request)
        {
            var course = await _context.Courses
                .Include(c => c.ResourceLinks)
                .SingleOrDefaultAsync(c => c.Id == request.ContextId);
            if (course == null)
            {
                return NotFound(default(LineItemContainer));
            }

            var lineitems = new LineItemContainer();
            foreach (var resourceLink in course.ResourceLinks)
            {
                var lineitem = new LineItem
                {
                    ResourceLinkId = resourceLink.Id.ToString(),
                    ScoreMaximum = 100
                };
                lineitem.Id = lineitem.GetHashCode().ToString();
                lineitems.Add(lineitem);
            }

            return Ok(lineitems);
        }
    }
}
