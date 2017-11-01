using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.LM
{
    /// <summary>
    /// LM standards for language model, it is an alternative to VSM in providing the retrieval/ranking function
    /// This approach use the statistical approach in NLP such as n-gram to find the (log) probability of a query given/assuming that
    /// the document is the correct document to retrieve (i.e., R = 1)
    /// That is:
    /// f(q, d) = log(P(q | d, R = 1)) = log(P(q | d)) = log(product_i(count(word_i, q) * P(word_i | d)))
    /// = sum_i(log(count(word_i, q) * P(word_i | d)))
    /// To resolve the issue with P(word_i | d) being zero forcing the f(q, d) when word_i is not found in d, that actual P(word_i | d) is
    /// formulated using smoothing as:
    /// P(word_i | d) = (1-alpha_d ) * P(word_i | d) + alpha_d * P(word_i | collection) 
    /// </summary>
    public abstract class UnigramLMRanking : IRanking
    {
        public abstract double Rank(ITokenizer query, ITokenizer doc, IEnumerable<ITokenizer> docs);
        public abstract double RankByWord(double count_word_in_query, double count_of_word_in_doc, double count_of_word_in_collection, int M, int dfW, int termCount, int queryLen, int docLen, double avdl);



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



        /// <summary>
        /// Return the probability of the unigram (i.e. the word) in the collection using the unigram collection language model (CollectionLM)
        /// </summary>
        /// <param name="word"></param>
        /// <param name="docs"></param>
        /// <returns></returns>
        public double GetUnigramProbability_CollectionLM(string word, IEnumerable<ITokenizer> docs)
        {
            int count_w_in_doc = 0;
            int count_w_in_collection = 0;
            int doc_length = 0;
            int collection_length = 0;
            foreach (ITokenizer doc in docs)
            {
                count_w_in_doc = GetTermFrequency(word, doc);
                doc_length = doc.Tokens.Length;

                count_w_in_collection += count_w_in_doc;
                collection_length += doc_length;
            }

            return (double)count_w_in_collection / collection_length;
        }

        protected int GetTermFrequency(string term, ITokenizer doc)
        {
            return GetTermFrequency(term, doc.Tokens);
        }

        protected int GetTermFrequency(string term, string[] words)
        {
            int word_count = words.Length;
            int freq = 0;
            for (int i = 0; i < word_count; ++i)
            {
                if (term == words[i])
                {
                    freq++;
                }
            }
            return freq;
        }
    }
}
