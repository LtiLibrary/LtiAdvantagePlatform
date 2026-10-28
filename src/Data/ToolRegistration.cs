using System;

namespace AdvantagePlatform.Data
{
    /// <summary>
    /// One LTI Dynamic Registration started with "Register a tool". The tool receives
    /// <see cref="Token"/> as the registration_token and sends it back as a Bearer token when
    /// it posts its configuration to the registration endpoint. The token says which user
    /// (platform tenant) the new tool belongs to, and can be used once.
    /// </summary>
    public class ToolRegistration
    {
        public int Id { get; set; }

        /// <summary>
        /// The registration_token: random, single use, short lived.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The user that started the registration and will own the tool.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The tool's registration initiation URL.
        /// </summary>
        public string InitiationUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The tool that registered with this token, once it has.
        /// </summary>
        public int? ToolId { get; set; }
    }
}
