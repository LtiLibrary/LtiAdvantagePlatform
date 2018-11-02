namespace AdvantagePlatform.Data
{
    public class ResourceLink
    {
        public int Id { get; set; }
        public LinkContexts? LinkContext { get; set; }
        public string Title { get; set; }
        public int ToolId { get; set; }
        public string UserId { get; set; }

        public enum LinkContexts
        {
            Course,
            Platform
        }
    }
}
