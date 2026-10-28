using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage.AspNetCore.AssignmentGradeServices;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
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
        private readonly CourseAccessValidator _courseValidator;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public LineItemsController(
            IWebHostEnvironment env,
            ILogger<LineItemsController> logger,
            ApplicationDbContext context,
            CourseAccessValidator courseValidator) : base(env, logger)
        {
            _context = context;
            _courseValidator = courseValidator;
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds a gradebook column to a course.
        /// </summary>
        /// <returns>The line item corresponding to the new gradebook column.</returns>
        protected override async Task<ActionResult<LineItem>> OnAddLineItemAsync(AddLineItemRequest request)
        {
            var (course, problem) = await _courseValidator.GetCourseAsync(request.ContextId, request.CancellationToken);
            if (problem != null)
            {
                return problem;
            }

            // Add a gradebook column to the course
            var gradebookColumn = new GradebookColumn
            {
                EndDateTime = request.LineItem.EndDateTime,
                Label = request.LineItem.Label,
                ResourceId = request.LineItem.ResourceId,
                ScoreMaximum = request.LineItem.ScoreMaximum,
                StartDateTime = request.LineItem.StartDateTime,
                Tag = request.LineItem.Tag
            };
            gradebookColumn.SetSubmissionReview(request.LineItem.SubmissionReviewExtension);

            if (request.LineItem.ResourceLinkId.IsPresent())
            {
                // The resource link must be one of this course's links.
                var resourceLink = course.ResourceLinks.SingleOrDefault(l => l.Id.ToString() == request.LineItem.ResourceLinkId);
                if (resourceLink == null)
                {
                    return ServiceProblems.BadRequest($"resourceLinkId {request.LineItem.ResourceLinkId} is not a link in this course.");
                }

                gradebookColumn.ResourceLink = resourceLink;
            }

            course.GradebookColumns.Add(gradebookColumn);

            await _context.SaveChangesAsync(request.CancellationToken);

            request.LineItem.Id = Url.Link(LtiAdvantage.Constants.ServiceEndpoints.Ags.LineItemService,
                new {contextId = request.ContextId, lineItemId = gradebookColumn.Id});

            return Created(request.LineItem.Id, request.LineItem);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Delete a gradebook column and the associated scores.
        /// </summary>
        protected override async Task<ActionResult> OnDeleteLineItemAsync(DeleteLineItemRequest request)
        {
            var (course, problem) = await _courseValidator.GetCourseAsync(request.ContextId, request.CancellationToken);
            if (problem != null)
            {
                return problem;
            }

            if (!int.TryParse(request.LineItemId, out var lineItemId) || course.GradebookColumns.All(c => c.Id != lineItemId))
            {
                return ServiceProblems.NotFound($"Line item {request.LineItemId} not found.");
            }

            var gradebookColumn = await _context.GetGradebookColumnAsync(lineItemId, request.CancellationToken);
            _context.GradebookColumns.Remove(gradebookColumn);
            await _context.SaveChangesAsync(request.CancellationToken);

            return NoContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a gradebook column from a course.
        /// </summary>
        /// <returns>The line item corresponding to the gradebook column.</returns>
        protected override async Task<ActionResult<LineItem>> OnGetLineItemAsync(GetLineItemRequest request)
        {
            var (course, problem) = await _courseValidator.GetCourseAsync(request.ContextId, request.CancellationToken);
            if (problem != null)
            {
                return problem;
            }

            if (!int.TryParse(request.LineItemId, out var lineItemId) || course.GradebookColumns.All(c => c.Id != lineItemId))
            {
                return ServiceProblems.NotFound($"Line item {request.LineItemId} not found.");
            }

            var gradebookColumn = course.GradebookColumns.Single(c => c.Id == lineItemId);

            return new LineItem
            {
                Id = Request.GetDisplayUrl(),
                EndDateTime = gradebookColumn.EndDateTime,
                Label = gradebookColumn.Label,
                ResourceId = gradebookColumn.ResourceId,
                ResourceLinkId = gradebookColumn.ResourceLink?.Id.ToString(),
                ScoreMaximum = gradebookColumn.ScoreMaximum,
                StartDateTime = gradebookColumn.StartDateTime,
                SubmissionReviewExtension = gradebookColumn.GetSubmissionReview(),
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
            var (course, problem) = await _courseValidator.GetCourseAsync(request.ContextId, request.CancellationToken);
            if (problem != null)
            {
                return problem;
            }

            var lineitems = new List<LineItem>();
            foreach (var gradebookColumn in course.GradebookColumns)
            {
                lineitems.Add(new LineItem
                {
                    Id = Url.Link(LtiAdvantage.Constants.ServiceEndpoints.Ags.LineItemService,
                        new {contextId = request.ContextId, lineItemId = gradebookColumn.Id}),
                    EndDateTime = gradebookColumn.EndDateTime,
                    Label = gradebookColumn.Label,
                    ResourceId = gradebookColumn.ResourceId,
                    ResourceLinkId = gradebookColumn.ResourceLink?.Id.ToString(),
                    ScoreMaximum = gradebookColumn.ScoreMaximum,
                    StartDateTime = gradebookColumn.StartDateTime,
                    SubmissionReviewExtension = gradebookColumn.GetSubmissionReview(),
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

            return new LineItemContainer(ServicePaging.Page(HttpContext, lineitems, request.Limit));
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates a line item.
        /// </summary>
        protected override async Task<ActionResult> OnUpdateLineItemAsync(UpdateLineItemRequest request)
        {
            var (course, problem) = await _courseValidator.GetCourseAsync(request.ContextId, request.CancellationToken);
            if (problem != null)
            {
                return problem;
            }

            if (!int.TryParse(request.LineItemId, out var lineItemId) || course.GradebookColumns.All(c => c.Id != lineItemId))
            {
                return ServiceProblems.NotFound($"Line item {request.LineItemId} not found.");
            }

            var gradebookColumn = await _context.GetGradebookColumnAsync(lineItemId, request.CancellationToken);
            gradebookColumn.EndDateTime = request.LineItem.EndDateTime;
            gradebookColumn.Label = request.LineItem.Label;
            gradebookColumn.ResourceId = request.LineItem.ResourceId;
            gradebookColumn.ScoreMaximum = request.LineItem.ScoreMaximum;
            gradebookColumn.StartDateTime = request.LineItem.StartDateTime;
            gradebookColumn.Tag = request.LineItem.Tag;
            gradebookColumn.SetSubmissionReview(request.LineItem.SubmissionReviewExtension);
            if (request.LineItem.ResourceLinkId.IsPresent())
            {
                // The resource link must be one of this course's links.
                var resourceLink = course.ResourceLinks.SingleOrDefault(l => l.Id.ToString() == request.LineItem.ResourceLinkId);
                if (resourceLink == null)
                {
                    return ServiceProblems.BadRequest($"resourceLinkId {request.LineItem.ResourceLinkId} is not a link in this course.");
                }

                gradebookColumn.ResourceLink = resourceLink;
            }
            else
            {
                gradebookColumn.ResourceLink = null;
            }

            _context.GradebookColumns.Update(gradebookColumn);

            await _context.SaveChangesAsync(request.CancellationToken);

            return NoContent();
        }
    }
}
