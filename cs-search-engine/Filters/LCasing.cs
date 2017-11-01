using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Filters
{
    public class LCasing : IFilter
    {
        public List<string> Filter(List<string> words)
        {
            List<string> result = new List<string>();
            foreach (string word in words)
            {
                result.Add(word.ToLower());
            }
            return result;
        }
    }
}
