using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4.EntityFramework.Entities;
using LtiAdvantage.IdentityServer4.Validation;

namespace AdvantagePlatform.Pages.Tools
{
    /// <summary>
    /// This Tool implements the "2.1.2.2 Tool registered and deployed" model shown in
    /// Figure 2 within https://www.imsglobal.org/spec/lti/v1p3#tool-registered-and-deployed
    /// </summary>
    public class ToolModel
    {
        /// <summary>
        /// Create an instance of <see cref="ToolModel"/>.
        /// </summary>
        public ToolModel() { }

        /// <summary>
        /// Create an instance of <see cref="ToolModel"/> using tool and client entities.
        /// </summary>
        /// <param name="tool">The tool entity.</param>
        /// <param name="client">The client entity.</param>
        public ToolModel(Tool tool, Client client)
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            if (client == null) throw new ArgumentNullException(nameof(client));

            Id = tool.Id;
            IdentityServerClientId = tool.IdentityServerClientId;

            ClientId = client.ClientId;
            CustomProperties = tool.CustomProperties;
            DeploymentId = tool.DeploymentId;
            LaunchUrl = tool.LaunchUrl;
            Name = tool.Name;
            PrivateKey = client.ClientSecrets
                ?.FirstOrDefault(s => s.Type == Constants.SecretTypes.PrivatePemKey)
                ?.Value;
        }

        /// <summary>
        /// The primary key.
        /// </summary>
        public int Id { get; set; }

        #region Identity Server Client properties

        /// <summary>
        /// The primary key of the IdentityServer Client associated with the tool.
        /// </summary>
        public int IdentityServerClientId { get; set; }

        /// <summary>
        /// The required Client ID. Must be unique in IdentityServer.
        /// </summary>
        [Required]
        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        /// <summary>
        /// The client private signing key.
        /// </summary>
        [Required]
        [Display(Name = "Private Key", Description = "Private key to sign messages sent by the tool.")]
        public string PrivateKey { get; set; }

        #endregion

        #region Tool properties

        /// <summary>
        /// Custom properties to include with all tool launches.
        /// </summary>
        [Display(Name = "Custom Properties", Description = "Custom properties to include in all tool launches.")]
        public string CustomProperties { get; set; }

        /// <summary>
        /// Tool launch url.
        /// </summary>
        [Required]
        [LocalhostUrl]
        [Display(Name = "Launch URL")]
        public string LaunchUrl { get; set; }

        /// <summary>
        /// Tool display name.
        /// </summary>
        [Required]
        [Display(Name = "Display Name")]
        public string Name { get; set; }

        #endregion

        #region Platform properties

        /// <summary>
        /// Generated and immutable deployment id.
        /// </summary>
        [Display(Name = "Deployment ID")]
        public string DeploymentId { get; set; }

        #endregion
    }
}
