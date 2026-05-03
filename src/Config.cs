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
            Constants.LtiScopes.Nrps.MembershipReadonly
        };
    }
}
