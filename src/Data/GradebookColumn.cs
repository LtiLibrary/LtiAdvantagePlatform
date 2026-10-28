using System;
using System.Collections.Generic;
using System.Linq;
using AdvantagePlatform.Utility;
using LtiAdvantage.SubmissionReview;

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
        /// The scores recorded in this gradebook column.
        /// </summary>
        public ICollection<GradebookRow> Scores { get; set; }

        /// <summary>
        /// The start date and time.
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Optional tag.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Submission Review: label of the "view submission" action. The tool sets the
        /// submissionReview property on the line item to say it can show a submission
        /// (https://www.imsglobal.org/spec/lti-sr/v1p0#line-item-submission-review-property).
        /// </summary>
        public bool HasSubmissionReview { get; set; }

        public string SubmissionReviewLabel { get; set; }

        /// <summary>
        /// Submission Review: the URL to launch. Empty means the resource link's launch URL.
        /// </summary>
        public string SubmissionReviewUrl { get; set; }

        /// <summary>
        /// Submission Review: custom parameters for the launch, one name=value per line.
        /// </summary>
        public string SubmissionReviewCustom { get; set; }

        /// <summary>
        /// The line item's submissionReview property, or null if the tool didn't set one.
        /// </summary>
        public SubmissionReviewProperty GetSubmissionReview()
        {
            if (!HasSubmissionReview) return null;

            SubmissionReviewCustom.TryConvertToDictionary(out var custom);
            return new SubmissionReviewProperty
            {
                Label = SubmissionReviewLabel,
                Url = SubmissionReviewUrl,
                Custom = custom
            };
        }

        public void SetSubmissionReview(SubmissionReviewProperty submissionReview)
        {
            HasSubmissionReview = submissionReview != null;
            SubmissionReviewLabel = submissionReview?.Label;
            SubmissionReviewUrl = submissionReview?.Url;
            SubmissionReviewCustom = submissionReview?.Custom == null
                ? null
                : string.Join("\r\n", submissionReview.Custom.Select(p => $"{p.Key}={p.Value}"));
        }
    }
}
