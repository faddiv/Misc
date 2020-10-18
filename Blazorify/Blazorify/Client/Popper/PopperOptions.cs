using System.Collections.Generic;

namespace Blazorify.Client.Popper
{
    public class PopperOptions
    {
        public string Placement { get; set; }

        public List<object> Modifiers { get; set; }

        public string Strategy { get; set; }
    }
}
