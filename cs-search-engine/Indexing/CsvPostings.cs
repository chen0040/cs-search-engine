using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SearchEngine.Indexing
{
    public class CsvPostings : IPostings
    {
        /// <summary>
        /// Postings: huge
        ///  -- Sequential access is expected
        ///  -- Can stay on disk
        ///  -- May contain docId, term freq., term pos, etc
        ///  -- compression is desirable
        /// </summary>
        protected List<PostingData> mPostings = new List<PostingData>();

        public int mBufferSize = 300;

        protected string mDirPath = "Postings";



        public string DirPath
        {
            get
            {
                return mDirPath;
            }
            set
            {
                mDirPath = value;
                if (!string.IsNullOrEmpty(mDirPath))
                {
                    if (!Directory.Exists(mDirPath))
                    {
                        Directory.CreateDirectory(mDirPath);
                    }
                }
            }
        }

        public void Add(int termId, int docId, int docLen, List<int> positions)
        {
            PostingData pd = new PostingData();
            pd.TermID = termId;
            pd.DocID = docId;
            pd.DocLen = docLen;
            pd.Positions.AddRange(positions);

            mPostings.Add(pd);

            if (mPostings.Count > mBufferSize)
            {
                Flush();
            }
        }

        public void Clear()
        {
            if (Directory.Exists(mDirPath))
            {
                Directory.Delete(mDirPath, true);
            }
            mPostings.Clear();
        }

        public void Load()
        {

        }

        public void Flush()
        {
            if (!string.IsNullOrEmpty(mDirPath))
            {
                if (!Directory.Exists(mDirPath))
                {
                    Directory.CreateDirectory(mDirPath);
                }
            }

            MergeSort(mPostings);
            int currTermId = -1;

            PostingData pd = null;
            TextWriter writer = null;
            for (int i = 0; i < mPostings.Count; ++i)
            {
                pd = mPostings[i];
                if (writer == null || currTermId != pd.TermID)
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                    currTermId = pd.TermID;
                    string filepath = string.Format("{0}.csv", currTermId);
                    if (!string.IsNullOrEmpty(mDirPath))
                    {
                        filepath = Path.Combine(mDirPath, filepath);
                    }
                    writer = new StreamWriter(filepath, true);
                }
                writer.WriteLine(string.Format("{0},{1},{2}", CompressDocId(pd.DocID), CompressDocLen(pd.DocLen), string.Join(":", pd.Positions)));
            }

            if (writer != null)
            {
                writer.Close();
            }

            mPostings.Clear();
        }

        private static void MergeSort(List<PostingData> postings)
        {
            int N = postings.Count;
            PostingData[] aux = new PostingData[N];
            MergeSort(postings, aux, 0, N-1);
        }

        private static void MergeSort(List<PostingData> postings, PostingData[] aux, int lo, int hi)
        {
            if (hi - lo < 3)
            {
                for (int l = lo; l < hi; ++l)
                {
                    int bestl2 = -1;
                    for (int l2 = l + 1; l2 <= hi; ++l2)
                    {
                        if (postings[l].TermID < postings[l2].TermID)
                        {
                            bestl2 = l2;
                        }
                    }
                    if (bestl2 != -1)
                    {
                        PostingData temp = postings[l];
                        postings[l] = postings[bestl2];
                        postings[bestl2] = temp;
                    }
                }
                return;
            }

            int mid = (hi + lo) / 2;
            MergeSort(postings, aux, lo, mid - 1);
            MergeSort(postings, aux, mid, hi);

            int i = lo, j = mid;
            for(int k=lo; k <= hi; ++k)
            {
                if ((j > hi) || (i < mid && postings[i].TermID <= postings[j].TermID))
                {
                    aux[k] = postings[i++];
                }
                else
                {
                    aux[k] = postings[j++];
                }
            }
            for (int k = lo; k <= hi; ++k)
            {
                postings[k] = aux[k];
            }
        }

        public string CompressDocId(int docId)
        {
            return docId.ToString(); //can implements gamma encoding here to compress the doc id
        }

        public string CompressDocLen(int docLen)
        {
            return docLen.ToString();
        }

        public int DecompressDocId(string docIdText)
        {
            return int.Parse(docIdText);
        }

        public int DecompressDocLen(string docLenText)
        {
            return int.Parse(docLenText);
        }

        public int GetWordCountInDoc(int termId, int docId)
        {
            int count = 0;

            for (int i = 0; i < mPostings.Count; ++i)
            {
                if (mPostings[i].TermID == termId)
                {
                    if (mPostings[i].DocID == docId)
                    {
                        count = mPostings[i].Positions.Count;
                        break;
                    }
                }
            }

            if (count == 0)
            {
                string compressedDocId = CompressDocId(docId) + ",";
                string filepath = Path.Combine(mDirPath, string.Format("{0}.csv", termId));

                using (TextReader reader = new StreamReader(filepath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith(compressedDocId))
                        {
                            string[] entries = line.Split(new char[] { ',' });
                            string[] positions = entries[2].Split(new char[] { ':' });
                            count = positions.Length;
                        }
                    }
                }
            }

            return count;
        }


        public List<int> GetWordPositionsInDoc(int termId, int docId)
        {
            List<int> positions = new List<int>();

            for (int i = 0; i < mPostings.Count; ++i)
            {
                if (mPostings[i].TermID == termId)
                {
                    if (mPostings[i].DocID == docId)
                    {
                        positions = mPostings[i].Positions;
                        break;
                    }
                }
            }

            if (positions.Count == 0)
            {
                string compressedDocId = CompressDocId(docId) + " ";
                string filepath = Path.Combine(mDirPath, string.Format("{0}.csv", termId));
                using (TextReader reader = new StreamReader(filepath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith(compressedDocId))
                        {
                           
                            string[] entries = line.Split(new char[] { ',' });
                            foreach (string pos in entries[2].Split(new char[] { ':' }))
                            {
                                positions.Add(int.Parse(pos));
                            }
                        }
                    }
                }
            }
            return positions;
        }


        public List<Tuple<int, int, int>> GetDocFreqs(int termId)
        {
            List<Tuple<int, int, int>> result = new List<Tuple<int, int, int>>();
   
            for (int i = 0; i < mPostings.Count; ++i)
            {
                if (mPostings[i].TermID == termId)
                {
                    result.Add(Tuple.Create(mPostings[i].DocID, mPostings[i].Positions.Count, mPostings[i].DocLen));
                }
            }

            string filepath = Path.Combine(mDirPath, string.Format("{0}.csv", termId));

            if (File.Exists(filepath))
            {
                using (TextReader reader = new StreamReader(filepath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] entries = line.Split(new char[] { ',' });
                        int docId = DecompressDocId(entries[0]);
                        int docLen = DecompressDocLen(entries[1]);
                        string[] positions = entries[2].Split(new char[] { ':' });
                        int count = positions.Length;
                        result.Add(Tuple.Create(docId, count, docLen));
                    }
                }
            }

            return result;
        }

    }
}
