using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Course
    {
        public Course()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Required]
        [Display(Name = "ID")]
        public string Id { get; set; }

        [Display(Name = "SIS ID")]
        public string SisId { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<ResourceLink> ResourceLinks { get; set; }
    }
}
