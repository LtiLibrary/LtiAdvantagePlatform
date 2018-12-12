using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Http.Extensions;
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

        protected override Task<LineItemResult> OnDeleteLineItemAsync(DeleteLineItemRequest request)
        {
            return Task.FromResult(LineItemOk());
        }

        protected override async Task<LineItemResult> OnGetLineItemAsync(GetLineItemRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return LineItemNotFound();
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == Convert.ToInt32(request.Id));
            if (gradebookColumn == null)
            {
                return LineItemNotFound();
            }

            return LineItemOk(new LineItem
            {
                Id = Request.GetDisplayUrl(),
                EndDateTime = gradebookColumn.EndDateTime,
                Label = gradebookColumn.Label,
                ResourceId = gradebookColumn.ResourceId,
                ResourceLinkId = gradebookColumn.ResourceLink.Id.ToString(),
                ScoreMaximum = gradebookColumn.ScoreMaximum,
                StartDateTime = gradebookColumn.StartDateTime,
                Tag = gradebookColumn.Tag
            });
        }

        protected override Task<LineItemResult> OnUpdateLineItemAsync(PutLineItemRequest request)
        {
            return Task.FromResult(LineItemOk());
        }
    }
}
