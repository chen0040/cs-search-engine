using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.LM
{
    /// <summary>
    /// Implements the Kullback-Leibler (LK) Divergence feedback model
    /// </summary>
    public class KLDivergenceFeedback : IFeedback
    {
        protected double mLambda = 0.1;
        protected Dictionary<string, double> mQueryVector;
        protected ITokenizer mQuery;

        public delegate double GetProbHandler_WordInCollection(string word);
        public event GetProbHandler_WordInCollection GetProb_WordInCollection;

        private static string[] GetTokenUnion(ITokenizer query, ITokenizer[] relevant_docs)
        {
            HashSet<string> word_union_set = new HashSet<string>();
            foreach (string word in query.Tokens)
            {
                word_union_set.Add(word);
            }

            foreach (ITokenizer t in relevant_docs)
            {
                foreach (string word in t.Tokens)
                {
                    word_union_set.Add(word);
                }
            }

            
            return word_union_set.ToArray();
        }

        public static HashSet<string> Clone(HashSet<string> a)
        {
            HashSet<string> clone = new HashSet<string>();
            foreach (string term in a)
            {
                clone.Add(term);
            }
            return clone;
        }

        /// <summary>
        /// Pseudo feedback can be the docs retrieved by the user's query in the previous instances.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pseudo_feedback"></param>
        /// <returns></returns>
        public Dictionary<string, double> BuildFromPseudoFeedback(ITokenizer query, ITokenizer[] pseudo_feedback)
        {
            mQuery = query;
            mQueryVector = new Dictionary<string, double>();

            string[] terms = GetTokenUnion(query, pseudo_feedback);

            HashSet<string> remaining = new HashSet<string>();
            foreach (string term in terms) remaining.Add(term);

            HashSet<string> theta = new HashSet<string>();
            double max_likelihood = 0;
            while (remaining.Count > 0)
            {
                double candidate_max_likelihood = 0;
                HashSet<string> best_candidate = null;
                foreach (string term in remaining)
                {
                    HashSet<string> candidate = Clone(theta);
                    candidate.Add(term);

                    double Prob_F_given_theta = CalcLikelihood_Feedback_given_theta(candidate, terms, pseudo_feedback);
                    if (candidate_max_likelihood < Prob_F_given_theta)
                    {
                        candidate_max_likelihood = Prob_F_given_theta;
                        best_candidate = candidate;
                    }
                }

                if (candidate_max_likelihood > max_likelihood)
                {
                    theta = best_candidate;
                    max_likelihood = candidate_max_likelihood;
                }
            }

            foreach (string word in theta)
            {
                mQueryVector[word] = 1;
            }

            return mQueryVector;
        }

        private double CalcLikelihood_Feedback_given_theta(HashSet<string> theta, string[] terms, ITokenizer[] pseudo_feedback)
        {
            int n = pseudo_feedback.Length;
            int W = terms.Length;
            int theta_length = theta.Count;
            double likelihood = 0;
            for (int i = 0; i < n; ++i)
            {
                ITokenizer doc_i = pseudo_feedback[i];
                for (int w = 0; w < W; ++w)
                {
                    string word = terms[w];
                    int count_word_in_doc = GetCount(word, doc_i);

                    double prob_word_in_theta = (theta.Contains(word) ? 1.0 : 0.0) / theta_length;
                    double prob_word_in_collection = GetProb_WordInCollection(word);

                    likelihood += count_word_in_doc * ((1 - mLambda) * prob_word_in_theta + mLambda * prob_word_in_collection);
                }
            }

            return likelihood;
        }

        private static int GetCount(string word, ITokenizer doc_i)
        {
            int count = 0;
            foreach (string term in doc_i.Tokens)
            {
                if (word == term)
                {
                    count++;
                }
            }
            return count;
        }

        public Dictionary<string, double> ModifiedQueryVector
        {
            get
            {
                return mQueryVector;
            }
        }

        public ITokenizer OriginalQuery
        {
            get { return mQuery; }
        }
    }
}
