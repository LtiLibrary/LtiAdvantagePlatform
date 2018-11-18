using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Data
{
    // Add profile data for application users by adding properties to the AdvantagePlatformUser class
    public class AdvantagePlatformUser : IdentityUser
    {
        public Course Course { get; set; }
        public Platform Platform { get; set; }
        public ICollection<Person> People { get; set; }
        public ICollection<ResourceLink> ResourceLinks { get; set; }
        public ICollection<Tool> Tools { get; set; }
    }
}
