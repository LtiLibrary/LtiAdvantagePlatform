namespace AdvantagePlatform.Data
{
    public class ResourceLink
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Custom properties included with this resource link.
        /// </summary>
        public string CustomProperties { get; set; }

        /// <summary>
        /// The context for this resource link (course or platform).
        /// </summary>
        public LinkContexts? LinkContext { get; set; }

        /// <summary>
        /// The resource link title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The tool used by this resource link.
        /// </summary>
        public int ToolId { get; set; }
        public Tool Tool { get; set; }

        /// <summary>
        /// The user that created this resource link.
        /// </summary>
        public AdvantagePlatformUser User { get; set; }

        public enum LinkContexts
        {
            Course,
            Platform
        }
    }
}
