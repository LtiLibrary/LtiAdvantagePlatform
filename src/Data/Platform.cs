﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AdvantagePlatform.Utility;

namespace AdvantagePlatform.Data
{
    public class Platform
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "GUID")]
        public string Guid { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Product Family Code")]
        public string ProductFamilyCode { get; set; }

        [LocalhostUrl]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [Display(Name = "Version")]
        public string Version { get; set; }

        public ICollection<ResourceLink> ResourceLinks { get; set; }
    }
}
