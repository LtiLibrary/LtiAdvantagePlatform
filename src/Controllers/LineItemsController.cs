using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
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

        protected override Task<LineItemResult> OnCreateLineItemAsync(PostLineItemRequest request)
        {
            var lineItem = request.LineItem;
            lineItem.Id = lineItem.GetHashCode().ToString();
            return Task.FromResult(Created(lineItem));
        }

        protected override Task<IActionResult> OnDeleteLineItemAsync(DeleteLineItemRequest request)
        {
            return Task.FromResult(new OkResult() as IActionResult);
        }

        protected override async Task<LineItemResult> OnGetLineItemAsync(GetLineItemRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound(default(LineItem));
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == Convert.ToInt32(request.Id));
            if (gradebookColumn == null)
            {
                return NotFound(default(LineItem));
            }

            return Ok(new LineItem
            {
                Id = Request.GetDisplayUrl().EnsureTrailingSlash() + gradebookColumn.Id,
                EndDateTime = gradebookColumn.EndDateTime,
                Label = gradebookColumn.Label,
                ResourceId = gradebookColumn.ResourceId,
                ResourceLinkId = gradebookColumn.ResourceLink.Id.ToString(),
                ScoreMaximum = gradebookColumn.ScoreMaximum,
                StartDateTime = gradebookColumn.StartDateTime,
                Tag = gradebookColumn.Tag
            });
        }

        protected override Task<IActionResult> OnUpdateLineItemAsync(PutLineItemRequest request)
        {
            return Task.FromResult(new OkResult() as IActionResult);
        }

        protected override async Task<LineItemContainerResult> OnGetLineItemsAsync(GetLineItemsRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound(default(LineItemContainer));
            }

            var lineitems = new LineItemContainer();
            foreach (var gradebookColumn in course.GradebookColumns)
            {
                lineitems.Add(new LineItem
                {
                    Id = Request.GetDisplayUrl().EnsureTrailingSlash() + gradebookColumn.Id,
                    EndDateTime = gradebookColumn.EndDateTime,
                    Label = gradebookColumn.Label,
                    ResourceId = gradebookColumn.ResourceId,
                    ResourceLinkId = gradebookColumn.ResourceLink.Id.ToString(),
                    ScoreMaximum = gradebookColumn.ScoreMaximum,
                    StartDateTime = gradebookColumn.StartDateTime,
                    Tag = gradebookColumn.Tag
                });
            }

            return Ok(lineitems);
        }
    }
}
