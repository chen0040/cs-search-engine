using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
namespace SearchEngine.LM
{
    /// <summary>
    /// This is the Jelinik-Mercer smoothing over the unigram language model used as ranking function. In JM smoothing, the probability
    /// P(word | doc) = (1-lambda) * P_seen(word | doc) + lambda * P(word | collection)
    /// </summary>
    public class UnigramLMWithJMSmoothing : UnigramLMRanking
    {
        protected double mLambda = 0.5; // value between 0 and 1

        public double Lambda
        {
            get { return mLambda; }
            set { mLambda = value; }
        }

        public override double Rank(ITokenizer query, ITokenizer doc, IEnumerable<ITokenizer> docs)
        {
            List<string> words = GetIntersection(query, doc);

            double logp = 0;

            int doc_length = doc.Tokens.Length;

            int word_count = words.Count;

            double lambda_term = (1 - mLambda) / mLambda;

            for (int i = 0; i < word_count; ++i)
            {
                string word = words[i];
                int count_w_in_q = GetTermFrequency(word, query);
                int count_w_in_doc = GetTermFrequency(word, doc);

                double Prob_w_in_collection = GetUnigramProbability_CollectionLM(word, docs);

                logp += count_w_in_q * System.Math.Log(1 + lambda_term * (count_w_in_doc / (doc_length * Prob_w_in_collection)));
            }
            return logp;
        }

        public override double RankByWord(double count_word_in_query, double count_of_word_in_doc, double count_of_word_in_collection, int M, int dfW, int termCount, int queryLen, int docLen, double avdl)
        {
            double lambda_term = (1 - mLambda) / mLambda;
            double Prob_w_in_collection = count_of_word_in_collection / (avdl * M);
            return count_word_in_query * System.Math.Log(1 + lambda_term * (count_of_word_in_doc / (docLen * Prob_w_in_collection)));
        }

    }
}
