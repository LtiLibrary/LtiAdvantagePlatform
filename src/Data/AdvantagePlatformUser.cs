using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace AdvantagePlatform.Data
{
    // Add profile data for application users by adding properties to the AdvantagePlatformUser class
    public class AdvantagePlatformUser : IdentityUser
    {
        public string CourseId { get; set; }
        public string PlatformId { get; set; }
        public string StudentId { get; set; }
        public string TeacherId { get; set; }
    }
}
