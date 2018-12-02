using System.Collections.Generic;
using IdentityServer4.Models;
using LtiAdvantage;

namespace AdvantagePlatform
{
    public class Config
    {
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            var lti = new IdentityResource(
                name: "lti",
                displayName: "LTI Advantage",
                claimTypes: new []{ "FullName" });

            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                lti
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(Constants.LtiScopes.NrpsMembershipReadonly, "Membership.Readonly"),
                new ApiResource(Constants.LtiScopes.AgsLineItem, "LineItem"),
                new ApiResource(Constants.LtiScopes.AgsResultReadonly, "Result.Readonly"),
                new ApiResource(Constants.LtiScopes.AgsScoreWriteonly, "Score")
            };
        }
    }
}