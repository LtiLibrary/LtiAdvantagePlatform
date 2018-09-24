using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Platform
    {
        public Platform()
        {
            PlatformId = Guid.NewGuid().ToString("N");
        }

        [Display(Name = "Platform ID")]
        public string PlatformId { get; set; }

        [Display(Name = "Platform Private Key")]
        public string PrivateKey { get; set; }

        [Display(Name = "Platform Public Key")]
        public string PublicKey { get; set; }

        public string UserId { get; set; }
    }
}
