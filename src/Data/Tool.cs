using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Tool
    {
        public int Id { get; set; }
        
        /// <summary>
        /// The ID of the AdvantagePlatformUser that created this Tool.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The Tool name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The URL used to launch the Tool.
        /// </summary>
        [Url]
        [Required]
        [Display(Name = "URL")]
        public string Url { get; set; }
    }
}
