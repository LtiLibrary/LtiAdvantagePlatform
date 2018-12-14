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

        protected override async Task<ActionResult<LineItem>> OnCreateLineItemAsync(CreateLineItemRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            var resourceLink = await _context.GetResourceLinkAsync(request.LineItem.ResourceLinkId);

            // And add a gradebook column to the course
            var gradebookColumn = new GradebookColumn
            {
                EndDateTime = request.LineItem.EndDateTime,
                Label = request.LineItem.Label,
                ResourceLink = resourceLink,
                ScoreMaximum = request.LineItem.ScoreMaximum,
                StartDateTime = request.LineItem.StartDateTime,
                Tag = request.LineItem.Tag
            };
            course.GradebookColumns.Add(gradebookColumn);

            await _context.SaveChangesAsync();

            request.LineItem.Id = Url.Link(LtiAdvantage.Constants.ServiceEndpoints.AgsLineItemService,
                new {request.ContextId, gradebookColumn.Id});

            return Created(request.LineItem.Id, request.LineItem);
        }

        protected override async Task<ActionResult<LineItemContainer>> OnGetLineItemsAsync(GetLineItemsRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound();
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

            return lineitems;
        }
    }
}
