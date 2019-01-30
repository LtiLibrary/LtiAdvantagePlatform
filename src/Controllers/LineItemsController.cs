using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
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
        private readonly CourseAccessValidator _courseValidator;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public LineItemsController(
            IHostingEnvironment env,
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
            if (!int.TryParse(request.ContextId, out var contextId))
            {
                var name = $"{nameof(request)}.{nameof(request.ContextId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a course id.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            
            if (!await _courseValidator.UserHasAccess(contextId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "User not authorized to access the requested course."
                });
            }

            var course = await _context.GetCourseAsync(contextId);
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
                if (!int.TryParse(request.LineItem.ResourceLinkId, out var resourceLinkId))
                {
                    var name = $"{nameof(request)}.{nameof(request.LineItem.ResourceLinkId)}";
                    ModelState.AddModelError(name, $"The {name} field cannot be converted into a resource link id.");
                    return BadRequest(new ValidationProblemDetails(ModelState));
                }

                var resourceLink = await _context.GetResourceLinkAsync(resourceLinkId);
                if (resourceLink == null)
                {
                    var name = $"{nameof(request)}.{nameof(request.LineItem)}.{request.LineItem.ResourceLinkId}";
                    ModelState.AddModelError(name, $"The {name} field is not a valid resource link id.");
                    return BadRequest(new ValidationProblemDetails(ModelState));
                }

                gradebookColumn.ResourceLink = resourceLink;
            }

            course.GradebookColumns.Add(gradebookColumn);

            await _context.SaveChangesAsync();

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
            if (!int.TryParse(request.ContextId, out var contextId))
            {
                var name = $"{nameof(request)}.{nameof(request.ContextId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a course id.");
            }

            if (!int.TryParse(request.LineItemId, out var lineItemId))
            {
                var name = $"{nameof(request)}.{nameof(request.LineItemId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a gradebook column id.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            
            if (!await _courseValidator.UserHasAccess(contextId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "User not authorized to access the requested course."
                });
            }

            var course = await _context.GetCourseAsync(contextId);
            if (course == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title= ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Course not found"
                });
            }

            if (course.GradebookColumns.Any(c => c.Id == lineItemId))
            {
                var gradebookColumn = await _context.GetGradebookColumnAsync(lineItemId);
                _context.GradebookColumns.Remove(gradebookColumn);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            return NotFound(new ProblemDetails
            {
                Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                Detail = "Gradebook column not found"
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a gradebook column from a course.
        /// </summary>
        /// <returns>The line item corresponding to the gradebook column.</returns>
        protected override async Task<ActionResult<LineItem>> OnGetLineItemAsync(GetLineItemRequest request)
        {
            if (!int.TryParse(request.ContextId, out var contextId))
            {
                var name = $"{nameof(request)}.{nameof(request.ContextId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a course id.");
            }

            if (!int.TryParse(request.LineItemId, out var lineItemId))
            {
                var name = $"{nameof(request)}.{nameof(request.LineItemId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a gradebook column id.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            
            if (!await _courseValidator.UserHasAccess(contextId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "User not authorized to access the requested course."
                });
            }

            var course = await _context.GetCourseAsync(contextId);
            if (course == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Course not found"
                });
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == lineItemId);
            if (gradebookColumn == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Gradebook column not found"
                });
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
            if (!int.TryParse(request.ContextId, out var contextId))
            {
                var name = $"{nameof(request)}.{nameof(request.ContextId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a course id.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            
            if (!await _courseValidator.UserHasAccess(contextId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "User not authorized to access the requested course."
                });
            }

            var course = await _context.GetCourseAsync(contextId);
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
                    Id = Url.Link(LtiAdvantage.Constants.ServiceEndpoints.Ags.LineItemService,
                        new {contextId = request.ContextId, lineItemId = gradebookColumn.Id}),
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

        /// <inheritdoc />
        /// <summary>
        /// Updates a line item.
        /// </summary>
        protected override async Task<ActionResult> OnUpdateLineItemAsync(UpdateLineItemRequest request)
        {
            if (!int.TryParse(request.ContextId, out var contextId))
            {
                var name = $"{nameof(request)}.{nameof(request.ContextId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a course id.");
            }

            if (!int.TryParse(request.LineItemId, out var lineItemId))
            {
                var name = $"{nameof(request)}.{nameof(request.LineItemId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a gradebook column id.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
                        
            if (!await _courseValidator.UserHasAccess(contextId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "User not authorized to access the requested course."
                });
            }

            var course = await _context.GetCourseAsync(contextId);
            if (course == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title= ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Course not found"
                });
            }

            if (course.GradebookColumns.All(c => c.Id != lineItemId))
            {
                return NotFound(new ProblemDetails
                {
                    Title= ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Gradebook column not found"
                });
            }

            var gradebookColumn = await _context.GetGradebookColumnAsync(lineItemId);
            gradebookColumn.EndDateTime = request.LineItem.EndDateTime;
            gradebookColumn.Label = request.LineItem.Label;
            gradebookColumn.ResourceId = request.LineItem.ResourceId;
            gradebookColumn.ScoreMaximum = request.LineItem.ScoreMaximum;
            gradebookColumn.StartDateTime = request.LineItem.StartDateTime;
            gradebookColumn.Tag = request.LineItem.Tag;
            if (request.LineItem.ResourceLinkId.IsPresent())
            {
                if (!int.TryParse(request.LineItem.ResourceLinkId, out var resourceLinkId))
                {
                    var name = $"{nameof(request)}.{nameof(request.LineItem)}.{request.LineItem.ResourceLinkId}";
                    ModelState.AddModelError(name, $"The {name} field cannot be converted into a valid resource link id.");
                    return BadRequest(new ValidationProblemDetails(ModelState));
                }

                var resourceLink = await _context.GetResourceLinkAsync(resourceLinkId);
                if (resourceLink == null)
                {
                    var name = $"{nameof(request)}.{nameof(request.LineItem)}.{request.LineItem.ResourceLinkId}";
                    ModelState.AddModelError(name, $"The {name} field is not a valid resource link id.");
                    return BadRequest(new ValidationProblemDetails(ModelState));
                }

                gradebookColumn.ResourceLink = resourceLink;
            }
            else
            {
                gradebookColumn.ResourceLink = null;
            }

            _context.GradebookColumns.Update(gradebookColumn);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
