using System.Collections.Generic;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.Components.Gradebook
{
    public class GradebookModel
    {
        public Dictionary<int, string> Members { get; set; }
        public List<MyGradebookColumn> Columns { get; set; }

        public class MyGradebookColumn
        {
            public string Header { get; set; }
            public GradebookColumn Column { get; set; }
        }
    }
}
