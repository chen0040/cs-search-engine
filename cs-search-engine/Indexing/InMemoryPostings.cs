using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SearchEngine.Indexing
{
    public class InMemoryPostings : IPostings
    {
        /// <summary>
        /// Postings: huge
        ///  -- Sequential access is expected
        ///  -- Can stay on disk
        ///  -- May contain docId, term freq., term pos, etc
        ///  -- compression is desirable
        /// </summary>
        protected Dictionary<int, Dictionary<int, List<int>>> mPostings = new Dictionary<int, Dictionary<int, List<int>>>();

        protected Dictionary<int, int> mDocLens = new Dictionary<int, int>();

        public void Clear()
        {
            mPostings.Clear();
            mDocLens.Clear();
        }

        protected string mDirPath = "MemIndexer";
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

        public void Flush()
        {
            if (!string.IsNullOrEmpty(mDirPath))
            {
                if (!Directory.Exists(mDirPath))
                {
                    Directory.CreateDirectory(mDirPath);
                }
            }

            int currTermId = -1;

            Dictionary<int, List<int>> pd = null;
            TextWriter writer = null;
            foreach (int termID in mPostings.Keys)
            {
                pd = mPostings[termID];
                if (writer == null || currTermId != termID)
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                    currTermId = termID;

                    string filepath = string.Format("{0}.mp", currTermId);
                    if (!string.IsNullOrEmpty(mDirPath))
                    {
                        filepath = Path.Combine(mDirPath, filepath);
                    }
                    writer = new StreamWriter(filepath, true);
                }
                foreach (int docID in pd.Keys)
                {
                    writer.WriteLine(string.Format("{0},{1},{2}", CompressDocId(docID), CompressDocLen(mDocLens[docID]), string.Join(":", pd[docID])));
                }
            }

            if (writer != null)
            {
                writer.Close();
            }
        }

        public void Load()
        {
            mDocLens.Clear();
            mPostings.Clear();

            string dirpath = "";
            if (string.IsNullOrEmpty(mDirPath))
            {
                dirpath = Directory.GetCurrentDirectory();
            }
            else
            {
                dirpath = mDirPath;
            }

            string[] files = Directory.GetFiles(dirpath, "*.mp");
            foreach (string filepath in files)
            {
                string filename = Path.GetFileNameWithoutExtension(filepath);
                int termId = int.Parse(filename);
                Dictionary<int, List<int>> termDict = new Dictionary<int, List<int>>();
                mPostings[termId] = termDict;

                using (StreamReader reader = new StreamReader(filepath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] comps = line.Split(new char[] { ',' });
                        int docId = int.Parse(comps[0]);
                        int docLen = int.Parse(comps[1]);
                        mDocLens[docId] = docLen;
                        string[] position_texts = comps[2].Split(new char[] { ':' });
                        List<int> positions = new List<int>();
                        foreach (string position_text in position_texts)
                        {
                            int position = int.Parse(position_text);
                            positions.Add(position);
                        }
                        termDict[docId] = positions;
                    }
                }
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

        public void Add(int termId, int docId, int docLen, List<int> positions)
        {
            Dictionary<int, List<int>> postionsInDoc = null;
            if (mPostings.ContainsKey(termId))
            {
                postionsInDoc = mPostings[termId];
            }
            else
            {
                postionsInDoc = new Dictionary<int, List<int>>(); ;
                mPostings[termId] = postionsInDoc;
            }

            postionsInDoc[docId] = positions;

            mDocLens[docId] = docLen;
        }

        public int GetWordCountInDoc(int termId, int docId)
        {
            Dictionary<int, List<int>> termFreqInDoc;
            if (mPostings.TryGetValue(termId, out termFreqInDoc))
            {
                List<int> positions; ;
                if (termFreqInDoc.TryGetValue(docId, out positions))
                {
                    return positions.Count;
                }
            }
            return 0;
        }


        public List<int> GetWordPositionsInDoc(int termId, int docId)
        {
            Dictionary<int, List<int>> termFreqInDoc;
            if (mPostings.TryGetValue(termId, out termFreqInDoc))
            {
                List<int> positions; ;
                if (termFreqInDoc.TryGetValue(docId, out positions))
                {
                    return positions;
                }
            }
            return null;
        }


        public List<Tuple<int, int, int>> GetDocFreqs(int termId)
        {
            List<Tuple<int, int, int>> result = new List<Tuple<int, int, int>>();
            Dictionary<int, List<int>> termFreqInDoc;
            if (mPostings.TryGetValue(termId, out termFreqInDoc))
            {
                foreach (int docId in termFreqInDoc.Keys)
                {
                    result.Add(Tuple.Create(docId, termFreqInDoc[docId].Count, mDocLens[docId]));
                }
            }
            return result;
        }
    }
}
