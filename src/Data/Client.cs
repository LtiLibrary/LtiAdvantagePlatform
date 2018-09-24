using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Client
    {
        public int Id { get; set; }
        public string CreatorId { get; set; }

        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Url(ErrorMessage = "You must supply a valid URL.")]
        [Display(Name = "Redirect URL")]
        public string RedirectUrl { get; set; }
    }
}
