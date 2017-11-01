using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine
{
    public interface IFeedback
    {
        Dictionary<string, double> ModifiedQueryVector { get; }
        ITokenizer OriginalQuery { get; }
    }
}
