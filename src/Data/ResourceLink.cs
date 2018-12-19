namespace AdvantagePlatform.Data
{
    /// <summary>
    /// An LTI resource link. This can belong to the platform (e.g. a district
    /// reporting tool) or to a course (e.g. an assignment).
    /// </summary>
    public class ResourceLink
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Custom properties included with this resource link.
        /// </summary>
        public string CustomProperties { get; set; }

        /// <summary>
        /// The link description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The link title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The tool to launch.
        /// </summary>
        public Tool Tool { get; set; }
    }
}
