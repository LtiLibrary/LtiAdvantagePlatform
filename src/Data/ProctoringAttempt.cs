using System;
using System.Collections.Generic;
using LtiAdvantage.Proctoring;

namespace AdvantagePlatform.Data
{
    /// <summary>
    /// One attempt at a proctored assessment (https://www.imsglobal.org/spec/proctoring/v1p0).
    /// Created when the platform sends LtiStartProctoring, started when the proctoring tool
    /// sends LtiStartAssessment back, and changed by the tool through the Assessment Control
    /// service while it runs.
    /// </summary>
    public class ProctoringAttempt
    {
        public int Id { get; set; }

        /// <summary>
        /// The proctored resource link (the assessment).
        /// </summary>
        public ResourceLink ResourceLink { get; set; }

        public int ResourceLinkId { get; set; }

        /// <summary>
        /// The candidate.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Starts at 1 and goes up with every attempt of this person at this assessment.
        /// </summary>
        public int AttemptNumber { get; set; }

        /// <summary>
        /// Opaque value sent with LtiStartProctoring. The tool must send it back unchanged with
        /// LtiStartAssessment, which ties the two messages to this attempt.
        /// </summary>
        public string SessionData { get; set; }

        /// <summary>
        /// The state of the attempt, as reported to the Assessment Control service.
        /// None until the proctoring tool sends LtiStartAssessment.
        /// </summary>
        public AssessmentControlStatus Status { get; set; } = AssessmentControlStatus.None;

        /// <summary>
        /// Extra minutes the proctoring tool granted with "update" actions.
        /// </summary>
        public int ExtraTime { get; set; }

        /// <summary>
        /// Whether the tool asked for an LtiEndAssessment message when the attempt ends.
        /// </summary>
        public bool EndAssessmentReturn { get; set; }

        /// <summary>
        /// The name the proctoring tool says it verified, from the verified_user claim.
        /// </summary>
        public string VerifiedUserName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// The Assessment Control actions the tool sent for this attempt.
        /// </summary>
        public ICollection<ProctoringEvent> Events { get; set; } = new List<ProctoringEvent>();
    }

    /// <summary>
    /// An Assessment Control action (pause, resume, terminate, update, flag) the proctoring
    /// tool sent for an attempt, kept to show in the platform UI.
    /// </summary>
    public class ProctoringEvent
    {
        public int Id { get; set; }

        public int ProctoringAttemptId { get; set; }

        public AssessmentControlAction Action { get; set; }

        public DateTime ReceivedAt { get; set; }

        public DateTime? IncidentTime { get; set; }

        public double? IncidentSeverity { get; set; }

        public int? ExtraTime { get; set; }

        public string ReasonCode { get; set; }

        public string ReasonMessage { get; set; }

        /// <summary>
        /// The attempt's status after the action.
        /// </summary>
        public AssessmentControlStatus ResultingStatus { get; set; }
    }
}
