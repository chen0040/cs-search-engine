using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.VSM
{
    /// <summary>
    /// BM25F is used for ranking documents with structures.
    /// a doc in this case is a set of fields, each field contains unstructured text
    /// </summary>
    public class BM25FieldsRanking : BM25Ranking
    {
        /// <summary>
        /// doc consists a set of fields, each field is represented by a ITokenizer
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected ITokenizer Transform(List<ITokenizer> doc)
        {
            ITokenizer doc2 = doc[0].Clone();
            doc2.Clear();
            int fieldCount = doc.Count;
            List<string> result = new List<string>();
            for (int i = 0; i < fieldCount; ++i)
            {
                ITokenizer field = doc[i];
                HashSet<string> words = new HashSet<string>();
                foreach (string word in field.Tokens)
                {
                    words.Add(word);
                }
                foreach (string word in words)
                {
                    result.Add(word);
                }
            }
            doc2.Tokens = result.ToArray();
            return doc2;
        }

        public virtual double Rank(ITokenizer query, List<ITokenizer> doc, IEnumerable<ITokenizer> docs)
        {
            ITokenizer doc2 = Transform(doc);
            return Rank(query, doc2, docs);
        }
    }
}
