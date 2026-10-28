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
        /// Custom property variables the platform fills in (without the leading "$"), see
        /// LtiAdvantageClaimsHandler.SubstituteCustomProperties. Advertised to Dynamic
        /// Registration and listed next to the Custom Properties fields.
        /// </summary>
        public static readonly IReadOnlyCollection<string> SupportedVariables = new[]
        {
            "User.id", "User.username",
            "Person.sourcedId", "Person.name.given", "Person.name.family", "Person.name.full", "Person.email.primary",
            "Context.id", "Context.id.history", "Context.title", "Context.sourcedId", "Context.type",
            "CourseSection.sourcedId", "CourseSection.title",
            "Membership.personSourcedId", "Membership.role", "Membership.status", "Membership.course.groupIds",
            "CourseGroup.id",
            "ResourceLink.id", "ResourceLink.id.history", "ResourceLink.title", "ResourceLink.description",
            "Message.documentTarget", "Message.height", "Message.width", "Message.locale", "Message.returnUrl",
            "ToolPlatform.productFamilyCode", "ToolPlatform.version",
            "ToolPlatformInstance.guid", "ToolPlatformInstance.name", "ToolPlatformInstance.description",
            "ToolPlatformInstance.url", "ToolPlatformInstance.contactEmail"
        };

        /// <summary>
        /// Help text for the Custom Properties fields.
        /// </summary>
        public const string CustomPropertiesHelp =
            "<p>Put each name=value pair on a separate line.</p>" +
            "<p>A value can be a variable the platform fills in at launch, for example <code>user=$User.username</code>. " +
            "Supported: $User.*, $Person.*, $Context.*, $CourseSection.*, $Membership.*, $CourseGroup.id, " +
            "$ResourceLink.*, $Message.*, $ToolPlatform.*, $ToolPlatformInstance.* (see the README for the full list).</p>";

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
