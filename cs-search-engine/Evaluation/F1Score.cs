using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Evaluation
{
    public class F1Score
    {
        public double GetF1Score(double precision, double recall)
        {
            return 2 * precision * recall / (precision + recall);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relevant_count">The number of relevant documents retrieve among the total number of documents retrieved</param>
        /// <param name="total_retrieval_count">The total number of documents retrieved</param>
        /// <param name="expected_max_relevant_count">The expected maximum number of relevant documents that should be retrieved</param>
        /// <returns></returns>
        public double GetF1Score(int relevant_count, int total_retrieval_count, int expected_max_relevant_count)
        {
            double precision = Precision.GetPrecision(relevant_count, total_retrieval_count);
            double recall = Recall.GetRecall(relevant_count, expected_max_relevant_count);
            return GetF1Score(precision, recall);
        }
    }
}
