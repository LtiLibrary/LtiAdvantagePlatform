using System.Collections.Generic;

namespace AdvantagePlatform.Data
{
    /// <summary>
    /// A named set of groups in a course, for example "Project teams"
    /// (https://www.imsglobal.org/spec/lti-gs/v1p0#group-sets).
    /// </summary>
    public class CourseGroupSet
    {
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Optional tag a tool can use to find the set.
        /// </summary>
        public string Tag { get; set; }

        public ICollection<CourseGroup> Groups { get; set; }
    }
}
