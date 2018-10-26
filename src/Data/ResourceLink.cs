using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class ResourceLink
    {
        public int Id { get; set; }

        /// <summary>
        /// The placement of this Resource Link. Can be either the user's
        /// platform or the user's course.
        /// </summary>
        [Required]
        [Display(Name = "Context")]
        public LinkContexts? LinkContext { get; set; }

        /// <summary>
        /// The title of this Resource Link
        /// </summary>
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// The Tool to use with this Resource Link
        /// </summary>
        public int ToolId { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that created this ResourceLink.
        /// </summary>
        public string UserId { get; set; }

        public enum LinkContexts
        {
            Course,
            Platform
        }
    }
}
