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

        [Required]
        [Display(Name = "Person is a Student")]
        public bool IsStudent { get; set; }
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "SIS ID")]
        public string SisId { get; set; }

        /// <summary>
        /// The local ID of the AdvantagePlatformUser that created this Person.
        /// </summary>
        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }
    }
}
