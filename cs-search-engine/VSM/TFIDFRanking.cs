using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SearchEngine.VSM
{
    /// <summary>
    /// Ranking function by term frequency - inverse document frequency (TD-IDF)
    /// </summary>
    public class TFIDFRanking : IRanking
    {
        /// <summary>
        /// The rank function f(q, d) is a similarity function which measures how similar is q and d.
        /// The higher the rank the more simliar is the document to the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="doc"></param>
        /// <param name="docs"></param>
        /// <returns></returns>
        public virtual double Rank(ITokenizer query, ITokenizer doc, IEnumerable<ITokenizer> docs)
        {
            List<string> common_words = GetIntersection(query, doc); 
            int N = common_words.Count;

            double f = 0;
            for (int i = 0; i < N; ++i)
            {
                string term = common_words[i];

                int dfW, M;
                GetHitDocCount(term, docs, out dfW, out M);

                f += GetCount(term, query) * GetCount(term, doc) * System.Math.Log((double)(M+1) / dfW);
            }
            return f;
        }

        public virtual double RankByWord(double count_word_in_query, double count_of_word_in_doc, double count_of_word_in_collection, int M, int dfW, int termCount, int queryLen, int docLen, double avdl)
        {
            return count_word_in_query * count_of_word_in_doc * System.Math.Log((double)(M + 1) / dfW);
        }

        protected virtual List<string> GetIntersection(ITokenizer query, ITokenizer doc)
        {
            HashSet<string> terms_query = new HashSet<string>();
            foreach (string term in query.Tokens)
            {
                terms_query.Add(term);
            }
            HashSet<string> terms_doc = new HashSet<string>();
            foreach (string term in doc.Tokens)
            {
                terms_doc.Add(term);
            }
            terms_query.IntersectWith(terms_doc);

            return terms_query.ToList();
        }

        public double[] Rank(Corpus corpus, ITokenizer doc, IEnumerable<ITokenizer> docs)
        {
            double[] itidf = new double[corpus.Count];
            int index = 0;
            foreach (string term in corpus)
            {
                int hitDocCount, totalDocCount;
                GetHitDocCount(term, docs, out hitDocCount, out totalDocCount);

                itidf[index++] = GetCount(term, doc) * GetIDF(hitDocCount, totalDocCount);
            }

            return itidf;
        }

        protected virtual void GetHitDocCount(string term, IEnumerable<ITokenizer> docs, out int hitDocCount, out int totalDocCount)
        {
            hitDocCount = 0;
            totalDocCount = 0;

            foreach (ITokenizer doc in docs)
            {
                totalDocCount++;
                string[] tokens = doc.Tokens;
                bool hit = false;
                for (int i = 0; i < tokens.Length; ++i)
                {
                    if (term == tokens[i])
                    {
                        hit = true;
                        break;
                    }
                }
                
                if (hit) hitDocCount++;
            }
        }

        /// <summary>
        /// TF Weight: Reward More frequent term in a doc
        /// </summary>
        /// <param name="term"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected virtual int GetCount(string term, ITokenizer doc)
        {
            string[] tokens = doc.Tokens;
            int docFreq = 0;
            for (int i = 0; i < tokens.Length; ++i)
            {
                if (term == tokens[i])
                {
                    docFreq++;
                }
            }
            return docFreq;
            
        }

        /// <summary>
        /// IDF Weighting: Penalizing Popular Terms
        /// IDF(W) = log[M / (k+1)]
        /// M: total number of docs in collection
        /// k: total number focs containing the term W (i.e. the Doc Frequency)
        /// </summary>
        /// <param name="hitDocCount"></param>
        /// <param name="totalDocCount"></param>
        /// <returns></returns>
        protected virtual double GetIDF(int hitDocCount, int totalDocCount)
        {
            return System.Math.Log((double)totalDocCount / (1+hitDocCount));
        }
    }
}
