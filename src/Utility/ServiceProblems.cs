using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdvantagePlatform.Utility
{
    /// <summary>
    /// The expected failures of the LTI service endpoints, as ProblemDetails. Unexpected
    /// exceptions are not handled here: the library's controller bases log them and answer
    /// 500 (with details only in Development).
    /// </summary>
    public static class ServiceProblems
    {
        /// <summary>
        /// 404: the course, line item, score, person or attempt doesn't exist.
        /// </summary>
        public static ActionResult NotFound(string detail) =>
            Problem(StatusCodes.Status404NotFound, "Not Found", detail);

        /// <summary>
        /// 403: the course exists, but doesn't belong to the tool (or user) that asks.
        /// </summary>
        public static ActionResult Forbidden(string detail = "The course does not belong to this tool.") =>
            Problem(StatusCodes.Status403Forbidden, "Forbidden", detail);

        /// <summary>
        /// 400: the request body or query is invalid.
        /// </summary>
        public static ActionResult BadRequest(string detail) =>
            Problem(StatusCodes.Status400BadRequest, "Bad Request", detail);

        private static ActionResult Problem(int status, string title, string detail) =>
            new ObjectResult(new ProblemDetails { Status = status, Title = title, Detail = detail })
            {
                StatusCode = status
            };
    }
}
