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

        [Display(Name = "Course ID")]
        public string Id { get; set; }

        [Display(Name = "Course Label")]
        public string Label { get; set; }

        [Display(Name = "Course Title")]
        public string Title { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that created this Course.
        /// </summary>
        [Display(Name = "User ID")]
        public string UserId { get; set; }
    }
}
