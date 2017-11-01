using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.VSM
{
    /// <summary>
    /// TF-IDF ranking with BM25 TF transform and document length normalization
    /// BM25F seems to be the most popular ranking function at the moment.
    /// </summary>
    public class BM25Ranking : TDIDFWithDocLengthNormalization
    {
      
        
        /// <summary>
        /// BM25 TF transform factor, k, k \in [0, PositiveInfinity], in which TF(word, doc) = (k+1)*count(word, doc) / (count(word, doc) + k)
        /// count(word, doc) is the frequency of word appearing in doc.
        /// The smaller the k value, the higher weights are place 
        /// </summary>
        protected double mTFTransformFactor = 1;
        public double TFTransformFactor
        {
            get { return mTFTransformFactor; }
            set { mTFTransformFactor = value; }
        }

        /// <summary>
        /// The higher the ranking score, the better the match between query and doc
        /// </summary>
        /// <param name="query"></param>
        /// <param name="doc"></param>
        /// <param name="docs"></param>
        /// <returns></returns>
        public override double Rank(ITokenizer query, ITokenizer doc, IEnumerable<ITokenizer> docs)
        {
            List<string> common_words = GetIntersection(query, doc);
            int N = common_words.Count;

            double k = mTFTransformFactor;
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

                f += count_word_in_query * (k + 1) * count_word_in_doc / (count_word_in_doc + k * GetNormalizedDocLength(dLen, avdl, b)) * System.Math.Log((double)(M + 1) / dfW);
            }
            return f;
        }

        public override double RankByWord(double count_word_in_query, double count_of_word_in_doc, double count_of_word_in_collection, int M, int dfW, int termCount, int queryLen, int docLen, double avdl)
        {
            double k = mTFTransformFactor;
            double b = DocumentLengthNormalizationBias;

            return count_word_in_query * (k + 1) * count_of_word_in_doc / (count_of_word_in_doc + k * GetNormalizedDocLength(docLen, avdl, b)) * System.Math.Log((double)(M + 1) / dfW);
        }
    }
}
