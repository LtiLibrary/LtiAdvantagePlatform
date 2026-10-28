using System.Collections.Generic;
using LtiAdvantage;
using OpenIddict.Abstractions;

namespace AdvantagePlatform
{
    /// <summary>
    /// Configuration data shared by Startup and the OpenIddict authorization flow.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Scopes supported by the platform.
        /// </summary>
        public static readonly IReadOnlyCollection<string> LtiScopes = new[]
        {
            OpenIddictConstants.Scopes.OpenId,
            Constants.LtiScopes.Ags.LineItem,
            Constants.LtiScopes.Ags.LineItemReadonly,
            Constants.LtiScopes.Ags.ResultReadonly,
            Constants.LtiScopes.Ags.Score,
            Constants.LtiScopes.Ags.ScoreReadonly,
            Constants.LtiScopes.Nrps.MembershipReadonly,
            Constants.LtiScopes.Gs.ContextGroupReadonly,
            Constants.LtiScopes.Ap.ControlAll
        };

        /// <summary>
        /// The product family code and version in the LTI platform configuration
        /// (Dynamic Registration) and in the platform claim of launches.
        /// </summary>
        public const string ProductFamilyCode = "LTI Advantage Platform";
        public const string PlatformVersion = "1.0";

        /// <summary>
        /// LTI messages this platform can send to tools, as advertised to Dynamic Registration.
        /// </summary>
        public static readonly IReadOnlyCollection<string> MessagesSupported = new[]
        {
            Constants.Lti.LtiResourceLinkRequestMessageType,
            Constants.Lti.LtiDeepLinkingRequestMessageType,
            Constants.Lti.LtiSubmissionReviewRequestMessageType,
            Constants.Lti.LtiStartProctoringRequestMessageType,
            Constants.Lti.LtiEndAssessmentRequestMessageType
        };
    }
}
