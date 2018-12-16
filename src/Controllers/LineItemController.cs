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
    public class LineItemController : LineItemControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LineItemController(
            ILogger<LineItemsControllerBase> logger, 
            ApplicationDbContext context) : base(logger)
        {
            _context = context;
        }

        protected override async Task<ActionResult> OnDeleteLineItemAsync(DeleteLineItemRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound();
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == Convert.ToInt32(request.Id));
            if (gradebookColumn == null)
            {
                return NotFound();
            }

            _context.GradebookColumns.Remove(gradebookColumn);
            await _context.SaveChangesAsync();

            return Ok();
        }

        protected override async Task<ActionResult> OnUpdateLineItemAsync(UpdateLineItemRequest request)
        {
            if (!Uri.TryCreate(request.LineItem.Id, UriKind.Absolute, out var lineItemId))
            {
                return BadRequest();
            }

            var pathStrings = lineItemId.GetLeftPart(UriPartial.Path).Split("/");
            var id = pathStrings[pathStrings.Length];

            var column = await _context.GetGradebookColumnAsync(id);

            column.EndDateTime = request.LineItem.EndDateTime;
            column.Label = request.LineItem.Label;
            column.ResourceId = request.LineItem.ResourceId;
            column.ScoreMaximum = request.LineItem.ScoreMaximum;
            column.StartDateTime = request.LineItem.StartDateTime;
            column.Tag = request.LineItem.Tag;
            if (request.LineItem.ResourceLinkId.IsPresent())
            {
                column.ResourceLink = await _context.GetResourceLinkAsync(request.LineItem.ResourceLinkId);
            }

            _context.GradebookColumns.Update(column);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
