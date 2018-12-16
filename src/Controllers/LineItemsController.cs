using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Sample implementation of a line items controller.
    /// </summary>
    public class LineItemsController : LineItemsControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public LineItemsController(
            IHostingEnvironment env,
            ILogger<ILineItemsController> logger,
            ApplicationDbContext context) : base(env, logger)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds a gradebook column to a course.
        /// </summary>
        /// <returns>The line item corresponding to the new gradebook column.</returns>
        protected override async Task<ActionResult<LineItem>> OnAddLineItemAsync(AddLineItemRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title= ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Course not found"
                });
            }

            // Add a gradebook column to the course
            var gradebookColumn = new GradebookColumn
            {
                EndDateTime = request.LineItem.EndDateTime,
                Label = request.LineItem.Label,
                ScoreMaximum = request.LineItem.ScoreMaximum,
                StartDateTime = request.LineItem.StartDateTime,
                Tag = request.LineItem.Tag
            };
            if (request.LineItem.ResourceLinkId.IsPresent())
            {
                var resourceLink = await _context.GetResourceLinkAsync(request.LineItem.ResourceLinkId);
                if (resourceLink == null)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest),
                        Detail = "Resource link not found"
                    });
                }

                gradebookColumn.ResourceLink = resourceLink;
            }

            course.GradebookColumns.Add(gradebookColumn);

            await _context.SaveChangesAsync();

            request.LineItem.Id = Url.Link(LtiAdvantage.Constants.ServiceEndpoints.AgsLineItemService,
                new {request.ContextId, gradebookColumn.Id});

            return Created(request.LineItem.Id, request.LineItem);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Returns a gradebook column from a course.
        /// </summary>
        /// <returns>The line item corresponding to the gradebook column.</returns>
        protected override async Task<ActionResult<LineItem>> OnGetLineItemAsync(GetLineItemRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound();
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == Convert.ToInt32(request.LineItemId));
            if (gradebookColumn == null)
            {
                return NotFound();
            }

            return new LineItem
            {
                Id = Request.GetDisplayUrl(),
                EndDateTime = gradebookColumn.EndDateTime,
                Label = gradebookColumn.Label,
                ResourceId = gradebookColumn.ResourceId,
                ResourceLinkId = gradebookColumn.ResourceLink.Id.ToString(),
                ScoreMaximum = gradebookColumn.ScoreMaximum,
                StartDateTime = gradebookColumn.StartDateTime,
                Tag = gradebookColumn.Tag
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns the gradebook columns in a course.
        /// </summary>
        /// <returns>Line items corresponding to the gradebook columns.</returns>
        protected override async Task<ActionResult<LineItemContainer>> OnGetLineItemsAsync(GetLineItemsRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title= ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Course not found"
                });
            }

            var lineitems = new List<LineItem>();
            foreach (var gradebookColumn in course.GradebookColumns)
            {
                lineitems.Add(new LineItem
                {
                    Id = Url.Link(LtiAdvantage.Constants.ServiceEndpoints.AgsLineItemService,
                        new {request.ContextId, gradebookColumn.Id}),
                    EndDateTime = gradebookColumn.EndDateTime,
                    Label = gradebookColumn.Label,
                    ResourceId = gradebookColumn.ResourceId,
                    ResourceLinkId = gradebookColumn.ResourceLink.Id.ToString(),
                    ScoreMaximum = gradebookColumn.ScoreMaximum,
                    StartDateTime = gradebookColumn.StartDateTime,
                    Tag = gradebookColumn.Tag
                });
            }

            if (request.ResourceId.IsPresent())
            {
                lineitems = lineitems.Where(l => l.ResourceId == request.ResourceId).ToList();
            }

            if (request.ResourceLinkId.IsPresent())
            {
                lineitems = lineitems.Where(l => l.ResourceLinkId == request.ResourceLinkId).ToList();
            }

            if (request.Tag.IsPresent())
            {
                lineitems = lineitems.Where(l => l.Tag == request.Tag).ToList();
            }

            return new LineItemContainer(lineitems);
        }
    }
}
