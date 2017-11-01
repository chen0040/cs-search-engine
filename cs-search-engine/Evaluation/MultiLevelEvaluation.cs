using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Evaluation
{
    /// <summary>
    /// In multilevel, the feedback from the user of the search engine include more than {relevant, not-relevant}, they can be 
    /// like {not-relevant, relevant, highly-relevant, jackpot}, in other words, the judgement criteria is multi-level
    /// </summary>
    public class MultiLevelEvaluation
    {
        /// <summary>
        /// Return the discounted cumulative gain for a multilevel judgement-based evaluation in which the level is represented by 1, 2, 3, with higher number corresponding to higher level
        /// </summary>
        /// <param name="scores">this is the user feedback in which each score can be 1, 2, 3, etc, with higher value indicates higher level</param>
        /// <returns></returns>
        public double GetDCG(int[] scores)
        {
            int n = scores.Length;
            double gain = 0;
            for (int i = 0; i < n; ++i)
            {
                if (i == 0)
                {
                    gain += scores[i];
                }
                else
                {
                    gain += scores[i] / System.Math.Log(i + 1);
                }
            }
            return gain;
        }

        public double GetNormalizedDCG(int[] scores, int[] optimal_scores_upper_bound)
        {
            int n = scores.Length;
            return GetDCG(scores) / GetDCG(optimal_scores_upper_bound);
        }
    }
}
