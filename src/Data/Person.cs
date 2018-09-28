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

        [Display(Name = "Person ID")]
        public string Id { get; set; }

        [Display(Name = "Person is a Student")]
        public bool IsStudent { get; set; }
        
        [Display(Name = "Person First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Person Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Person SIS ID")]
        public string SisId { get; set; }

        /// <summary>
        /// The local ID of the AdvantagePlatformUser that created this Person.
        /// </summary>
        [Display(Name = "User ID")]
        public string UserId { get; set; }
    }
}
