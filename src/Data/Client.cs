using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Client
    {
        public int Id { get; set; }

        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        [Required]
        [Display(Name = "Client Name")]
        public string Name { get; set; }

        [Display(Name = "Client Private Key")]
        public string PrivateKey { get; set; }

        [Display(Name = "Client Public Key")]
        public string PublicKey { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that created this Client.
        /// </summary>
        [Display(Name = "User ID")]
        public string UserId { get; set; }
    }
}
