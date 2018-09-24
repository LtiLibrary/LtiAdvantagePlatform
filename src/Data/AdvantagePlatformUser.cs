using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Data
{
    // Add profile data for application users by adding properties to the AdvantagePlatformUser class
    public class AdvantagePlatformUser : IdentityUser
    {
        public string PlatformId { get; set; }
    }
}
