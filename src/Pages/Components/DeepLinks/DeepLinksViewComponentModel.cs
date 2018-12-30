﻿using System.Collections.Generic;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;

namespace AdvantagePlatform.Pages.Components.DeepLinks
{
    public class DeepLinksViewComponentModel
    {
        public int? CourseId { get; set; }
        public IList<Person> People { get; set; }
        public IList<ToolModel> Tools { get; set; }
    }
}
