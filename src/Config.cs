using System.Collections.Generic;
using IdentityServer4.Models;
using LtiAdvantage;

namespace AdvantagePlatform
{
    /// <summary>
    /// Configuration data required at first startup of Identity Server.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Identity resources that can be requested.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId()
            };
        }

        /// <summary>
        /// List of API's that are protected.
        /// </summary>
        /// <returns></returns>
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