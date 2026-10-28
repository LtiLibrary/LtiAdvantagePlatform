using System.Collections.Generic;

namespace AdvantagePlatform.Data
{
    /// <summary>
    /// A group of people in a course, for example "Team Red"
    /// (https://www.imsglobal.org/spec/lti-gs/v1p0#groups).
    /// </summary>
    public class CourseGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Tag { get; set; }

        /// <summary>
        /// The set this group belongs to. (The spec allows a group in several sets; this
        /// sample keeps one.)
        /// </summary>
        public CourseGroupSet GroupSet { get; set; }

        public int GroupSetId { get; set; }

        public ICollection<Person> Members { get; set; }
    }
}
