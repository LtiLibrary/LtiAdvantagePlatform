namespace AdvantagePlatform.Data
{
    /// <summary>
    /// This Tool implements the "2.1.2.2 Tool registered and deployed" model shown in
    /// Figure 2 within https://www.imsglobal.org/spec/lti/v1p3#tool-registered-and-deployed
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Custom properties included with every resource link that uses this tool.
        /// </summary>
        public string CustomProperties { get; set; }

        /// <summary>
        /// The URL used to launch the deep linking flow.
        /// </summary>
        public string DeepLinkingLaunchUrl { get; set; }

        /// <summary>
        /// The Deployment ID for this Tool/Client combination.
        /// </summary>
        public string DeploymentId { get; set; }

        /// <summary>
        /// The primary key of the Identity Server client associated with this tool.
        /// Not the OIDC client id.
        /// </summary>
        public int IdentityServerClientId { get; set; }

        /// <summary>
        /// The URL used to launch the tool.
        /// </summary>
        public string LaunchUrl { get; set; }

        /// <summary>
        /// The URL used to initiate OIDC authorization..
        /// </summary>
        public string LoginUrl { get; set; }

        /// <summary>
        /// The tool name.
        /// </summary>
        public string Name { get; set; }
    }
}
