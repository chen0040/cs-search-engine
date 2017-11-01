using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Filters
{
    public interface IFilter
    {
        List<string> Filter(List<string> words);
    }
}
