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
        /// Where the platform shows the tool: "window" (a new tab, the default) or "iframe"
        /// (embedded in the platform page). Sent to the tool as launch_presentation.document_target.
        /// </summary>
        public string DocumentTarget { get; set; }

        /// <summary>
        /// Height in pixels of the iframe when <see cref="DocumentTarget"/> is "iframe".
        /// </summary>
        public int? IframeHeight { get; set; }

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
