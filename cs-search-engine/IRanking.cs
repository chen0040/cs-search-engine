using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine
{
    public interface IRanking
    {
        double Rank(ITokenizer query, ITokenizer doc, IEnumerable<ITokenizer> docs);
        double RankByWord(double count_word_in_query, double count_of_word_in_doc, double count_of_word_in_collection, int M, int dfW, int termCount, int queryLen, int docLen, double avdl);
    }
}
