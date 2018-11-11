using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4;
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
            ClientSecret = client.Properties
                ?.FirstOrDefault(p => p.Key == IdentityServerConstants.SecretTypes.SharedSecret)
                ?.Value;
            DeploymentId = tool.DeploymentId;
            Name = tool.Name;
            PrivateKey = client.ClientSecrets
                ?.FirstOrDefault(s => s.Type == Constants.SecretTypes.PrivateKey)
                ?.Value;
            PublicKey = client.ClientSecrets
                ?.FirstOrDefault(s => s.Type == Constants.SecretTypes.PublicKey)
                ?.Value;
            Url = tool.Url;
        }

        /// <summary>
        /// The primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The primary key of the IdentityServer client associated with the tool.
        /// </summary>
        public int IdentityServerClientId { get; set; }

        /// <summary>
        /// The required Client ID. Must be unique in IdentityServer.
        /// </summary>
        [Required]
        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        /// <summary>
        /// Optional shared secret.
        /// </summary>
        [Display(Name = "Client Secret", Description = "This is the shared secret to use if your tool sends a shared secret for client credentials.")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Generated and immutable deployment id.
        /// </summary>
        [Display(Name = "Deployment ID", Description = "This deployment ID will be sent with all launch messages from the platform.")]
        public string DeploymentId { get; set; }

        /// <summary>
        /// The display name of the tool.
        /// </summary>
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// The generated private key.
        /// </summary>
        [Display(Name = "Private Key", Description = "This is the private key to use if your tool sends a signed JWT as client credentials.")]
        public string PrivateKey { get; set; }

        /// <summary>
        /// The generated public key.
        /// </summary>
        [Display(Name = "Public Key")]
        public string PublicKey { get; set; }
        
        /// <summary>
        /// The tool launch url.
        /// </summary>
        [Required]
        [NullableUrl]
        [Display(Name = "Launch URL")]
        public string Url { get; set; }
    }
}
