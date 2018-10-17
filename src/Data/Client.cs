using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Client
    {
        public Client()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Required]
        [Display(Name = "ID")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that owns this Client.
        /// </summary>
        public string UserId { get; set; }
    }
}
