using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Indexing
{
    public interface IPostings
    {
        void Add(int termId, int docId, int docLen, List<int> positions);

        int GetWordCountInDoc(int termId, int docId);

        List<int> GetWordPositionsInDoc(int termId, int docId);

        List<Tuple<int, int, int>> GetDocFreqs(int termId);

        void Flush();
        void Load();
        void Clear();

        string DirPath
        {
            get;
            set;
        }
    }
}
