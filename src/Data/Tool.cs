namespace AdvantagePlatform.Data
{
    /// <summary>
    /// This Tool implements the "2.1.2.2 Tool registered and deployed" model shown in
    /// Figure 2 within https://www.imsglobal.org/spec/lti/v1p3#tool-registered-and-deployed
    /// </summary>
    public class Tool
    {
        public int Id { get; set; }

        /// <summary>
        /// The Deployment ID for this Tool/Client combination
        /// </summary>
        public string DeploymentId { get; set; }

        /// <summary>
        /// The ID of the IdentityServer Client associated with this Tool.
        /// </summary>
        public int IdentSvrClientId { get; set; }

        /// <summary>
        /// The Tool name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The URL used to launch the Tool.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that created this ResourceLink.
        /// </summary>
        public string UserId { get; set; }
    }
}
