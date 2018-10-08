using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Deployment
    {
        public Deployment()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Display(Name = "ID")]
        public string Id { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that created this Deployment.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The Client to use with this Deployment.
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// The Tool to use with this Deployment.
        /// </summary>
        public Tool Tool { get; set; }

        /// <summary>
        /// The Tool placement of this Deployment. Can be either the user's
        /// platform or the user's course.
        /// </summary>
        [Display(Name = "Placement")]
        public ToolPlacements? ToolPlacement { get; set; }

        public enum ToolPlacements
        {
            Course,
            Platform
        }
    }
}
