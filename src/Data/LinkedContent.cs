namespace AdvantagePlatform.Data
{
    /// <summary>
    /// A non-LTI content item a tool returned with Deep Linking: a link, an HTML fragment,
    /// a file or an image (https://www.imsglobal.org/spec/lti-dl/v2p0#content-item-types).
    /// LTI resource links are stored as <see cref="ResourceLink"/>s instead.
    /// </summary>
    public class LinkedContent
    {
        public int Id { get; set; }

        /// <summary>
        /// The content item type: "link", "html", "file" or "image".
        /// </summary>
        public string Type { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        /// <summary>
        /// The URL of the link, file or image.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The HTML fragment of an "html" item.
        /// </summary>
        public string Html { get; set; }

        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// How to show a link: "iframe" (embedded in the page) or "window" (new tab).
        /// </summary>
        public string DocumentTarget { get; set; }

        /// <summary>
        /// Size of the iframe when <see cref="DocumentTarget"/> is "iframe".
        /// </summary>
        public int? Width { get; set; }

        public int? Height { get; set; }
    }
}
