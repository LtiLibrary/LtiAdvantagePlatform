using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class MyClient
    {
        public MyClient()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Required]
        [Display(Name = "Client ID")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        public string Name { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that owns this Client.
        /// </summary>
        public string UserId { get; set; }
    }
}
