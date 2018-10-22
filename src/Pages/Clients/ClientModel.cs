using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Pages.Clients
{
    public class ClientModel
    {
        public int Id { get; set; }

        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string ClientName { get; set; }

        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }
    }
}
