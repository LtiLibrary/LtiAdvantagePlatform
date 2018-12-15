using System.Collections.Generic;
using IdentityModel;
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
        /// LTI scopes.
        /// </summary>
        public static ICollection<string> LtiScopes => new[]
        {
            OidcConstants.StandardScopes.OpenId,
            Constants.LtiScopes.AgsLineItem,
            Constants.LtiScopes.AgsLineItemReadonly,
            Constants.LtiScopes.AgsResultReadonly,
            Constants.LtiScopes.AgsScore,
            Constants.LtiScopes.AgsScoreReadonly,
            Constants.LtiScopes.NrpsMembershipReadonly
        };

        /// <summary>
        /// Built-in clients.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "swagger",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = LtiScopes,
                    RedirectUris = new [] { "/swagger/oauth2-redirect.html" }
                }
            };
        }

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
                new ApiResource(Constants.LtiScopes.AgsScore, "Score.Writeonly")
            };
        }
    }
}