using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SearchEngine.Tokenizers
{
    public class EnglishTokenizer : ITokenizer
    {
        private static Regex mRegexTarget = new Regex("[A-Za-z]+('[A-Za-z]*)?|[0-9]+(,[0-9]+)*(\\.[0-9]+)?|[!\"\"#$%&'()*+,-\\./:;<=>?@\\[\\\\\\]^_`{|}~]");
        private string[] mTokens = null;

        public EnglishTokenizer(string content)
        {
            mTokens = Tokenize(content);
        }

        public EnglishTokenizer()
        {

        }

        public EnglishTokenizer(string[] tokens)
        {
            if (tokens != null)
            {
                mTokens = (string[])tokens.Clone();
            }
        }

        public string[] Tokens
        {
            get { return mTokens; }
            set { mTokens = value; }
        }

        
        public string[] Tokenize(string content)
        {
            MatchCollection matches = mRegexTarget.Matches(content);
            string[] tokens = new string[matches.Count];
            for(int i=0; i < matches.Count; ++i)
            {
                Match m = matches[i];
                tokens[i] = m.Value;

            }
            return tokens;
        }






        public ITokenizer Clone()
        {
            return new EnglishTokenizer(mTokens);
        }


        public void Clear()
        {
            mTokens = null;
        }
    }
}
