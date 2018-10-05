using System;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Course
    {
        public Course()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Display(Name = "ID")]
        public string Id { get; set; }

        [Display(Name = "SIS ID")]
        public string SisId { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// The local ID of the AdvantagePlatformUser that created this Course.
        /// </summary>
        public string UserId { get; set; }
    }
}
