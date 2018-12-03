using System;
using LtiAdvantage.AssignmentGradeServices;

namespace AdvantagePlatform.Data
{
    /// <summary>
    /// Represents a row of the course gradebook. Each row will have
    /// 0 or more scores for a person in the course.
    /// </summary>
    public class GradebookRow
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Status of the user toward activity's completion.
        /// </summary>
        public ActivityProgress ActivityProgress { get; set; }

        /// <summary>
        /// A comment with the score.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The status of the grading process.
        /// </summary>
        public GradingProgess GradingProgress { get; set; }

        /// <summary>
        /// The score.
        /// </summary>
        public double ScoreGiven { get; set; }

        /// <summary>
        /// The maximum possible score.
        /// </summary>
        public double ScoreMaximum { get; set; }

        /// <summary>
        /// The UTC time the score was set. ISO 8601 format.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// The person id.
        /// </summary>
        public string PersonId { get; set; }
    }
}
