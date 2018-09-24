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
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string UserId { get; set; }
    }
}
