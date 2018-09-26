using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Platform
    {
        public Platform()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Display(Name = "Platform ID")]
        public string Id { get; set; }

        [Display(Name = "Platform Private Key")]
        public string PrivateKey { get; set; }

        [Display(Name = "Platform Public Key")]
        public string PublicKey { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that created this Platform.
        /// </summary>
        [Display(Name = "User ID")]
        public string UserId { get; set; }
    }
}
