using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.VSM
{
    /// <summary>
    /// Pivoted length normalization VSM by (Singhal et al 96)
    /// The TF transform is done by TF(word, doc) = ln[1+ln[1+count(word, doc)]]
    /// where count(word, doc) is the frequency of word appearing in doc
    /// </summary>
    public class PivotedLengthNormalizationVSM : TDIDFWithDocLengthNormalization
    {
        public override double Rank(ITokenizer query, ITokenizer doc, IEnumerable<ITokenizer> docs)
        {
            List<string> common_words = GetIntersection(query, doc);
            int N = common_words.Count;

            double b = DocumentLengthNormalizationBias;

            int dLen = doc.Tokens.Length;

            double avdl = GetAverageDocumentLength(docs);

            double f = 0;
            for (int i = 0; i < N; ++i)
            {
                string term = common_words[i];

                int dfW, M;
                GetHitDocCount(term, docs, out dfW, out M);

                int count_word_in_query = GetCount(term, query);
                int count_word_in_doc = GetCount(term, query);

                f += count_word_in_query * System.Math.Log(1 + System.Math.Log(1 + count_word_in_doc)) / GetNormalizedDocLength(dLen, avdl, b) * System.Math.Log((double)(M + 1) / dfW);
            }
            return f;
        }

        public override double RankByWord(double count_word_in_query, double count_of_word_in_doc, double count_of_word_in_collection, int M, int dfW, int termCount, int queryLen, int docLen, double avdl)
        {
            double b = DocumentLengthNormalizationBias;
            return count_word_in_query * System.Math.Log(1 + System.Math.Log(1 + count_of_word_in_doc)) / GetNormalizedDocLength(docLen, avdl, b) * System.Math.Log((double)(M + 1) / dfW);
        }
    }
}
