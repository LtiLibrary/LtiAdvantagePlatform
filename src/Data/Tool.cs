using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class Tool
    {
        public int Id { get; set; }

        /// <summary>
        /// The Tool name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The URL used to launch the Tool.
        /// </summary>
        [Url]
        [Display(Name = "URL")]
        public string Url { get; set; }
    }
}
