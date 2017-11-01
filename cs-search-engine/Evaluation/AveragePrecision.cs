using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Evaluation
{
    public class AveragePrecision
    {
        /// <summary>
        /// Return the average precision
        /// </summary>
        /// <param name="doc_relevants"></param>
        /// <returns></returns>
        public static double GetAP(bool[] doc_relevants)
        {
            int total_retrieval_count = doc_relevants.Length;

            int relevant_count = 0;
            int current_relevant_count = 0;
            double sum = 0;
            for (int i = 0; i < total_retrieval_count; ++i)
            {
                relevant_count += doc_relevants[i] ? 1 : 0;
                if (current_relevant_count != relevant_count) //cut point at which the (recall rate = relevant_count / max_relevant_count) changes
                {
                    int retrieval_count = i + 1;
                    current_relevant_count = relevant_count;
                    double precision = (double)current_relevant_count / relevant_count;
                    sum += precision;
                }
            }

            return sum / total_retrieval_count;
        }

        /// <summary>
        /// Return the mean average precision
        /// </summary>
        /// <param name="query_results"></param>
        /// <returns></returns>
        public double GetMAP(Dictionary<string, bool[]> query_results)
        {
            List<double> performances = new List<double>();
            foreach (string query in query_results.Keys)
            {
                double ap = GetAP(query_results[query]);
                performances.Add(ap);
            }
            return performances.Average();
        }
    }
}
