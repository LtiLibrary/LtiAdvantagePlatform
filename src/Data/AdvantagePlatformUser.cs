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

        [Obsolete("Only for persistence by Entity Framework")]
        public string ClientIdsForDatabase
        {
            get
            {
                return ClientIds == null || !ClientIds.Any()
                    ? null
                    : JsonConvert.SerializeObject(ClientIds);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    ClientIds.Clear();
                }
                else
                {
                    ClientIds = JsonConvert.DeserializeObject<List<int>>(value);
                }
            }
        }

        /// <summary>
        /// JSON list of <see cref="IdentityServer4.EntityFramework.Entities.Client.Id"/>
        /// added by this AdvantagePlatformUser.
        /// </summary>
        [NotMapped]
        public List<int> ClientIds { get; set; } = new List<int>();
    }
}
