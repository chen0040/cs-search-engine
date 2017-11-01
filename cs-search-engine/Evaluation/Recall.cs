using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Evaluation
{
    public class Recall
    {
        /// <summary>
        /// Return the recall rate of the search engine result
        /// </summary>
        /// <param name="relevant_count">The actual number of relevant documents retrieved</param>
        /// <param name="expected_max_relevant_count">The expected maximum number of relevant documents that should be retrieved</param>
        /// <returns></returns>
        public static double GetRecall(double relevant_count, double expected_max_relevant_count)
        {
            return (double)relevant_count / expected_max_relevant_count;
        }
    }
}
