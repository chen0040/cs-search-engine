using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine
{
    public interface ITokenizer
    {
        string[] Tokenize(string content);
        string[] Tokens
        {
            get;
            set;
        }

        ITokenizer Clone();
        void Clear();
    }
}
