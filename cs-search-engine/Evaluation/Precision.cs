using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Evaluation
{
    public class Precision
    {
        /// <summary>
        /// Return the precision of the search engine result
        /// </summary>
        /// <param name="relevant_count">The number of relevant documents retrieve among the total number of documents retrieved</param>
        /// <param name="total_retrieval_count">The total number of documents retrieved</param>
        /// <returns></returns>
        public static double GetPrecision(int relevant_count, int total_retrieval_count)
        {
            return (double)relevant_count / total_retrieval_count;
        }
    }
}
