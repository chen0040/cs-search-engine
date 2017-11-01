using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SearchEngine.Tokenizers
{
    public class PairedTokenizer
    {
        public List<PairedToken> Tokenize(string sentence)
        {
            List<PairedToken> results = new List<PairedToken>();

            string[] tokens = sentence.Split(new char[] { ' ', '\t', '\r', '\n' });

            for (int i = 0; i < tokens.Length; ++i)
            {
                string token = tokens[i];
                string[] wordTags = token.Split(new char[] { '/' });
                string word = wordTags[0];
                string tag = wordTags[1];
                PairedToken pair = new PairedToken()
                {
                    Word=word,
                    Tag = tag
                };
                results.Add(pair);
            }

            return results;
        }
    }
}
