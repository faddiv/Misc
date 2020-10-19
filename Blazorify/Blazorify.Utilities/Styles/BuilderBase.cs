using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazorify.Utilities.Styles
{
    public class BuilderBase<TBuilder>
        where TBuilder : BuilderBase<TBuilder>
    {
        public BuilderBase(string separator)
        {
            Separator = separator;
            Values = new List<string>();
        }

        public string Separator { get; }

        public List<string> Values { get; }


        public void Add(string value, Func<bool> predicate)
        {
            Add(value, predicate());
        }

        protected void Add(string value, bool condition = true)
        {
            if (!string.IsNullOrEmpty(value) && condition)
            {
                Values.Add(value);
            }
        }

        public override string ToString()
        {
            return string.Join(Separator, Values);
        }
    }
}
