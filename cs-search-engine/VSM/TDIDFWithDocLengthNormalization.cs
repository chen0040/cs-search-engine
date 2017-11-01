using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.VSM
{
    public class TDIDFWithDocLengthNormalization : TFIDFRanking
    {
        /// <summary>
        /// Document length normalization bias, b, b \in [0, 1]
        /// The document length normalization: 1-b+b(Length(doc) / avdl(docs))
        /// where avdl(docs) is the average length of all documents in the collection
        /// The larger the b value, the more bias is placed on shorter document (in other words, ranking will be higher for shorter documents, all other factors considered equal)
        /// </summary>
        protected double mDocumentLengthNormalizationBias = 0.5;
        public double DocumentLengthNormalizationBias
        {
            get { return mDocumentLengthNormalizationBias; }
            set { mDocumentLengthNormalizationBias = value; }
        }


        protected double mCachedAvdl = -1;
        protected bool mUseCachedAvdl = true;

        public bool UseCacheAvdl
        {
            get { return mUseCachedAvdl; }
            set { mUseCachedAvdl = value; }
        }

        protected double GetAverageDocumentLength(IEnumerable<ITokenizer> docs)
        {
            if (!mUseCachedAvdl || mCachedAvdl < 0)
            {
                double sum = 0;
                int count = 0;
                foreach (ITokenizer doc in docs)
                {
                    sum += doc.Tokens.Length;
                    count++;
                }
                mCachedAvdl = sum / count;
            }
            return mCachedAvdl;
        }

        /// <summary>
        /// Reset the average document length 
        /// </summary>
        public void ResetAvdl()
        {
            mCachedAvdl = -1;
        }

        /// <summary>
        /// The higher the rank the more simliar is the document to the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="doc"></param>
        /// <param name="docs"></param>
        /// <returns></returns>
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

                f += GetCount(term, query) * GetCount(term, doc) / GetNormalizedDocLength(dLen, avdl, b) * System.Math.Log((double)(M + 1) / dfW);
            }
            return f;
        }

        protected virtual double GetNormalizedDocLength(double dLen, double avdl, double b)
        {
            return (1 - b + b * dLen / avdl);
        }

        public override double RankByWord(double count_word_in_query, double count_of_word_in_doc, double count_of_word_in_collection, int M, int dfW, int termCount, int queryLen, int docLen, double avdl)
        {
            double b = DocumentLengthNormalizationBias;
            return count_word_in_query * count_of_word_in_doc / GetNormalizedDocLength(docLen, avdl, b) * System.Math.Log((double)(M + 1) / dfW);
        }

    }

}
