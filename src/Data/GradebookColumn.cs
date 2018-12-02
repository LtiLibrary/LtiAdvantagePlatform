using System;

namespace AdvantagePlatform.Data
{
    /// <summary>
    /// Represents a column of the course gradebook. Each row will have
    /// scores and results for each person in the course.
    /// </summary>
    public class GradebookColumn
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The end date and time.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Optional, human-friendly label for this LineItem suitable for display. 
        /// For example, this label might be used as the heading of a column in a gradebook.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The resource link.
        /// </summary>
        public ResourceLink ResourceLink { get; set; }

        /// <summary>
        /// The non-link resource id.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// The maximum score allowed.
        /// </summary>
        public double? ScoreMaximum { get; set; }

        /// <summary>
        /// The start date and time.
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Optional tag.
        /// </summary>
        public string Tag { get; set; }
    }
}
