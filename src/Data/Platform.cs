using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Platform
    {
        public Platform()
        {
            Id = System.Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Issuer (iss) for messages that originate from this platform.
        /// </summary>
        [Display(Name = "ID")]
        public string Id { get; set; }

        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "GUID")]
        public string Guid { get; set; }

        public KeyPair KeyPair { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Product Family Code")]
        public string ProductFamilyCode { get; set; }

        [Url]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [Display(Name = "Version")]
        public string Version { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that created this Platform.
        /// </summary>
        public string UserId { get; set; }
    }
}
