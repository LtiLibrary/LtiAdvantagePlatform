using System.Collections.Generic;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.Components.Gradebook
{
    public class GradebookViewComponentModel
    {
        public Dictionary<int, string> Members { get; set; }
        public List<MyGradebookColumn> Columns { get; set; }

        public int CourseId { get; set; }

        /// <summary>
        /// The person who launches Submission Review: the course's instructor.
        /// </summary>
        public int? InstructorId { get; set; }

        public class MyGradebookColumn
        {
            public string Header { get; set; }
            public GradebookColumn Column { get; set; }
        }
    }
}
