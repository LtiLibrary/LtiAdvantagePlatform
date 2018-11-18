using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4.EntityFramework.Entities;
using LtiAdvantage.IdentityServer4;

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
        public ToolModel() {}

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
            DeploymentId = tool.DeploymentId;
            JsonWebKeySetUrl = tool.JsonWebKeySetUrl;
            LaunchUrl = tool.LaunchUrl;
            Name = tool.Name;
            PublicKey = client.ClientSecrets
                ?.FirstOrDefault(s => s.Type == Constants.SecretTypes.PublicPemKey)
                ?.Value;
        }

        /// <summary>
        /// The primary key.
        /// </summary>
        public int Id { get; set; }

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
        /// Generated and immutable deployment id.
        /// </summary>
        [Display(Name = "Deployment ID", Description = "This deployment ID will be sent with all launch messages from the platform.")]
        public string DeploymentId { get; set; }
        
        /// <summary>
        /// The launch url.
        /// </summary>
        [Required]
        [NullableUrl]
        [Display(Name = "Launch URL")]
        public string LaunchUrl { get; set; }

        /// <summary>
        /// The display name of the tool.
        /// </summary>
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        
        [NullableUrl]
        [Display(Name = "JSON Web Key Set URL", Description = "URL to retrieve the tool's current public keys. If supplied, the current keys will be retrieved just prior to tool launch, to allow for frequent key rotation.")]
        public string JsonWebKeySetUrl { get; set; }
        
        [Display(Name = "Issuer", Description = "The Issuer for all launch messages from the platform.")]
        public string PlatformIssuer { get; set; }

        /// <summary>
        /// The public signing key.
        /// </summary>
        [Display(Name = "Public Key", Description = "Public key to validate messages signed by the tool.")]
        public string PublicKey { get; set; }
    }
}
