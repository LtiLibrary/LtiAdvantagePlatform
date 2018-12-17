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
                // Client for Swagger UI
                new Client
                {
                    ClientId = "swagger",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
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
                new ApiResource
                {
                    Name = Constants.ServiceEndpoints.AgsLineItemsService,
                    DisplayName = "LTI Assignment and Grade Line Item Service",
                    Description = "Provides tools access to gradebook columns",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = Constants.LtiScopes.AgsLineItem,
                            DisplayName = $"Full access to {Constants.ServiceEndpoints.AgsLineItemsService}",
                            Description = "Allow the tool to add, remove, change, and read gradebook columns"
                        },
                        new Scope
                        {
                            Name = Constants.LtiScopes.AgsLineItemReadonly,
                            DisplayName = $"Read only access to {Constants.ServiceEndpoints.AgsLineItemsService}",
                            Description = "Allow the tool to read gradebook columns"
                        }
                    }
                },
                new ApiResource
                {
                    Name = Constants.ServiceEndpoints.AgsResultsService,
                    DisplayName = "LTI Assignment and Grade Result Service",
                    Description = "Provides tools access to gradebook results",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = Constants.LtiScopes.AgsResultReadonly,
                            DisplayName = $"Read only access to {Constants.ServiceEndpoints.AgsResultsService}",
                            Description = "Allow the tool to read gradebook results"
                        }
                    }
                },
                new ApiResource
                {
                    Name = Constants.ServiceEndpoints.AgsScoresService,
                    DisplayName = "LTI Assignment and Grade Score Service",
                    Description = "Provides tools access to gradebook scores",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = Constants.LtiScopes.AgsScore,
                            DisplayName = $"Full access to {Constants.ServiceEndpoints.AgsScoresService}",
                            Description = "Allow the tool to add and read gradebook scores"
                        },
                        new Scope
                        {
                            Name = Constants.LtiScopes.AgsScoreReadonly,
                            DisplayName = $"Read only access to {Constants.ServiceEndpoints.AgsScoresService}",
                            Description = "Allow the tool to read gradebook scores"
                        }
                    }
                },
                new ApiResource
                {
                    Name = Constants.ServiceEndpoints.NrpsMembershipService,
                    DisplayName = "LTI Names and Role Provisioning Membership Service",
                    Description = "Provides tools access to course membership",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = Constants.LtiScopes.NrpsMembershipReadonly,
                            DisplayName = $"Read only access to {Constants.ServiceEndpoints.NrpsMembershipService}",
                            Description = "Allow the tool to see who is enrolled in a course"
                        }
                    }
                }
            };
        }
    }
}