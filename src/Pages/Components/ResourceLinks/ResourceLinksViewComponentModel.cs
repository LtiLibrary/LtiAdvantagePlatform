using System.Collections.Generic;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;

namespace AdvantagePlatform.Pages.Components.ResourceLinks
{
    public class ResourceLinksViewComponentModel
    {
        public int? CourseId { get; set; }
        public IList<Person> People { get; set; }
        public IList<ResourceLinkModel> ResourceLinks { get; set; }
        public IList<LinkedContent> LinkedContent { get; set; }

        /// <summary>
        /// Attempts at the proctored links, newest first.
        /// </summary>
        public IList<ProctoringAttempt> ProctoringAttempts { get; set; }
    }
}
