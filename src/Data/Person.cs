using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Person
    {
        public Person()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Required]
        [Display(Name = "ID")]
        public string Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Roles")]
        public string Roles { get; set; }

        [Display(Name = "SIS ID")]
        public string SisId { get; set; }

        [Display(Name = "Username")]
        public string Username { get; set; }

        public AdvantagePlatformUser User { get; set; }
    }
}
