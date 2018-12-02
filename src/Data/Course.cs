using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Course
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        [Required]
        [Display(Name = "ID")]
        public int Id { get; set; }

        /// <summary>
        /// SIS ID for the course.
        /// </summary>
        [Display(Name = "SIS ID")]
        public string SisId { get; set; }

        /// <summary>
        /// Name of the course.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Represents the columns of the course gradebook. Each row will have
        /// scores and results for each person in the course.
        /// </summary>
        public ICollection<GradebookColumn> GradebookColumns { get; set; }

        /// <summary>
        /// Represents the LTI resources in the course.
        /// </summary>
        public ICollection<ResourceLink> ResourceLinks { get; set; }
    }
}
