using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SearchEngine.Helpers;
using SearchEngine.VSM;
using System.IO;
using System.Xml;

namespace SearchEngine.Indexing
{
    /// <summary>
    /// Indexing is technique that converts docments to data strucure that enable fast search (precomputing as much as we can)
    /// inverted index is the domniating indexing method for supporting basic search algorithms
    /// 
    /// Zipf's Law: rank(word) * frequency(word) is roughly a constant
    /// </summary>
    public class InvertedIndexer
    {
        /// <summary>
        /// Dictionary containing the number of docs in which a particular word appears
        /// Dictionary: modest size
        ///  -- Needs fast random access
        ///  -- Preferred to be in memory
        ///  -- Implementation: hash table, b-tree, trie, ...
        /// </summary>
        protected Dictionary<int, int> mDocCount = new Dictionary<int, int>();
        protected Dictionary<string, int> mBagsOfWords = new Dictionary<string, int>();

        /// <summary>
        /// Average Document Length
        /// </summary>
        protected double mAVDL = 0;

        /// <summary>
        /// Postings: huge
        ///  -- Sequential access is expected
        ///  -- Can stay on disk
        ///  -- May contain docId, term freq., term pos, etc
        ///  -- compression is desirable
        /// </summary>
        protected IPostings mPostings = new InMemoryPostings();


        protected int mTotalDocCount = 0;

        public void Save(string filepath_docCount, string filepath_bagsOfWords, string filepath_config)
        {
            mPostings.Flush();

            Serialize(mDocCount, filepath_docCount);
            Serialize(mBagsOfWords, filepath_bagsOfWords);

            XmlDocument doc = new XmlDocument();
            XmlElement docRoot = doc.CreateElement("config");
            doc.AppendChild(docRoot);

            XmlAttribute docRootTotalDocCount = doc.CreateAttribute("totalDocCount");
            docRootTotalDocCount.Value = mTotalDocCount.ToString();
            docRoot.Attributes.Append(docRootTotalDocCount);

            XmlAttribute docRootAVDL = doc.CreateAttribute("avdl");
            docRootAVDL.Value = mAVDL.ToString();
            docRoot.Attributes.Append(docRootAVDL);

            doc.Save(filepath_config);
        }

        public void Load(string filepath_docCount, string filepath_bagsOfWords, string filepath_config)
        {
            mPostings.Load();

            Load(mDocCount, filepath_docCount);
            Load(mBagsOfWords, filepath_bagsOfWords);

            XmlDocument doc = new XmlDocument();
            doc.Load(filepath_config);

            XmlElement docRoot = doc.DocumentElement;
            mTotalDocCount = int.Parse(docRoot.Attributes["totalDocCount"].Value);
            mAVDL = double.Parse(docRoot.Attributes["avdl"].Value);

        }

        public void Clear(string filepath_docCount, string filepath_bagsOfWords, string filepath_config)
        {
            mPostings.Clear();

            if (File.Exists(filepath_docCount))
            {
                File.Delete(filepath_docCount);
            }
            if (File.Exists(filepath_bagsOfWords))
            {
                File.Delete(filepath_bagsOfWords);
            }

            if (File.Exists(filepath_config))
            {
                File.Delete(filepath_config);
            }

        }

        private static void Load(Dictionary<int, int> docCount, string filepath_docCount)
        {
            docCount.Clear();
            using (StreamReader reader = new StreamReader(filepath_docCount))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                  
                    string[] comps = line.Split(new char[]{','});
                    int key = int.Parse(comps[0]);
                    int value = int.Parse(comps[1]);
                    docCount[key] = value;
                }
            }
        }

        private static void Load(Dictionary<string, int> dict, string filepath)
        {
            dict.Clear();
            using (StreamReader reader = new StreamReader(filepath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(",")) line = "[commma]" + line.Substring(1);
                    string[] comps = line.Split(new char[] { ',' });
                    string key = comps[0];
                    int value = int.Parse(comps[1]);
                    dict[key] = value;
                }
            }
        }

        private static void Serialize(Dictionary<int, int> docCount, string filepath_docCount)
        {
            using (StreamWriter writer = new StreamWriter(filepath_docCount))
            {
                foreach (int termId in docCount.Keys)
                {
                    writer.WriteLine("{0},{1}", termId, docCount[termId]);
                }
            }
        }

        private static void Serialize(Dictionary<string, int> bagsOfWords, string filepath_bagsOfWords)
        {
            using (StreamWriter writer = new StreamWriter(filepath_bagsOfWords))
            {
                foreach (string word in bagsOfWords.Keys)
                {
                    writer.WriteLine("{0},{1}", word, bagsOfWords[word]);
                }
            }
        }

        /// <summary>
        /// Dictionary containing the total number times in which a particular word appears in the database
        /// Dictionary: modest size
        ///  -- Needs fast random access
        ///  -- Preferred to be in memory
        ///  -- Implementation: hash table, b-tree, trie, ...
        /// </summary>
        protected Dictionary<int, int> mTotalFreq = new Dictionary<int, int>();

        protected int Hash(string word)
        {
            int wordId;
            if(mBagsOfWords.TryGetValue(word, out wordId))
            {
                return wordId;
            }
            wordId = mBagsOfWords.Count;
            mBagsOfWords[word] = wordId;
            return wordId ;
        }

       

        /// <summary>
        /// Return a list of docs, sorted descendingly by their ranks, given a user-input query and with the feedback model
        /// </summary>
        /// <param name="query">user-input query</param>
        /// <param name="ranker">similarity criteria</param>
        /// <param name="resultCount"></param>
        /// <returns>List of docId, searchReults, with searchResults[0] having the highest rank</returns>
        public int[] SearchWithFeedback(IFeedback feedback, IRanking ranker, int resultCount, out double[] scores)
        {
            int queryLen = feedback.OriginalQuery.Tokens.Length;

            Dictionary<string, double> query_vector_0 = feedback.ModifiedQueryVector;
            Dictionary<int, double> query_vector = new Dictionary<int, double>();
            foreach (string term in query_vector_0.Keys)
            {
                int termId = Hash(term);
                query_vector[termId] = query_vector_0[term];
            }

            return Search(feedback.OriginalQuery, query_vector, ranker, resultCount, out scores);
        }

        protected int[] Search(ITokenizer query, Dictionary<int, double> query_vector, IRanking ranker, int resultCount, out double[] result_scores)
        {
            int queryLen = query.Tokens.Length;

            int[] terms = query_vector.Keys.ToArray();
            int termCount = terms.Length;

            Dictionary<int, double> ranks = new Dictionary<int, double>();

            for (int k = 0; k < termCount; ++k)
            {
                int termId = terms[k];

                List<Tuple<int, int, int>> docFreqs = mPostings.GetDocFreqs(termId);

                int count_of_word_in_collection = 0;
                mTotalFreq.TryGetValue(termId, out count_of_word_in_collection);

                for (int i = 0; i < docFreqs.Count; ++i)
                {
                    int docId = docFreqs[i].Item1;
                    int freq = docFreqs[i].Item2;
                    int docLen = docFreqs[i].Item3;

                    double wordRankWithDoc = ranker.RankByWord(query_vector[termId], freq, count_of_word_in_collection, mTotalDocCount, mDocCount[termId], termCount, queryLen, docLen, mAVDL);

                    if (ranks.ContainsKey(docId))
                    {
                        ranks[docId] += wordRankWithDoc;
                    }
                    else
                    {
                        ranks[docId] = wordRankWithDoc;
                    }
                }
            }

            MinPQ<int> heap = new MinPQ<int>();
            foreach (int docId in ranks.Keys)
            {
                if (heap.Count < resultCount)
                {
                    heap.Add(ranks[docId], docId);
                }
                else if (heap.Peek().Key < ranks[docId])
                {
                    heap.Add(ranks[docId], docId);
                    heap.DeleteMin();
                }
            }

            result_scores = new double[heap.Count];
            int[] result = new int[heap.Count];
            for (int i = 0; i < result.Length; ++i)
            {
                MinPQEntry<int> entry = heap.DeleteMin();
                result[result.Length - i - 1] = entry.Value;
                result_scores[result.Length - i - 1] = entry.Key;
            }

            return result;
        }

        /// <summary>
        /// Return a list of docs, sorted descendingly by their ranks, given a user-input query
        /// </summary>
        /// <param name="query">user-input query</param>
        /// <param name="ranker">similarity criteria</param>
        /// <param name="resultCount"></param>
        /// <returns>List of docId, searchReults, with searchResults[0] having the highest rank</returns>
        public int[] Search(ITokenizer query, IRanking ranker, int resultCount, out double[] scores)
        {
            string[] tokens = query.Tokens;
           

            Dictionary<int, double> query_vector = new Dictionary<int, double>();
            int queryLen = tokens.Length;
            for (int k = 0; k < queryLen; ++k)
            {
                string term = tokens[k];
                int termId = Hash(term);
                if (query_vector.ContainsKey(termId))
                {
                    query_vector[termId] += 1;
                }
                else
                {
                    query_vector[termId] = 1;
                }
            }

            return Search(query, query_vector, ranker, resultCount, out scores);
            
        }

      
        public void Index(ITokenizer doc, int docId)
        {
            string[] tokens = doc.Tokens;
            int docLen = tokens.Length;

            HashSet<int> uniqueWords = new HashSet<int>();
            Dictionary<int, List<int>> positions = new Dictionary<int, List<int>>();
            for(int i=0; i < docLen; ++i)
            {
                string term = tokens[i];
                int termId = Hash(term);

                if (!positions.ContainsKey(termId))
                {
                    positions[termId] = new List<int>();
                }

                if (uniqueWords.Contains(termId))
                {
                    positions[termId].Add(i);
                }
                else
                {
                    uniqueWords.Add(termId);
                    positions[termId].Add(i);

                    if (mDocCount.ContainsKey(termId))
                    {
                        mDocCount[termId] += 1;
                    }
                    else
                    {
                        mDocCount[termId] = 1;
                    }
                }
            }

            foreach (int termId in uniqueWords)
            {
                int termFreq = positions[termId].Count;

                if (mTotalFreq.ContainsKey(termId))
                {
                    mTotalFreq[termId] += termFreq;
                }
                else
                {
                    mTotalFreq[termId] = termFreq;
                }

                mPostings.Add(termId, docId, docLen, positions[termId]);
            }

            UpdateDocLenStats(docLen);
            
        }

        private void UpdateDocLenStats(int docLen)
        {
            mAVDL = (mAVDL * mTotalDocCount + docLen) / (mTotalDocCount+1);
            mTotalDocCount++;
        }

        public int GetWordCountInDoc(string term, int docId)
        {
            int termId = Hash(term);
            return mPostings.GetWordCountInDoc(termId, docId);
        }

        public List<int> GetWordPositionsInDoc(string term, int docId)
        {
            int termId = Hash(term);
            return mPostings.GetWordPositionsInDoc(termId, docId);
        }

        public int GetDocFreq(string term)
        {
            int count = 0;
            int termId = Hash(term);
            if (mDocCount.TryGetValue(termId, out count))
            {
                return count;
            }
            return 0;
        }

        public int GetTotalFreq(string term)
        {
            int count = 0;
            int termId = Hash(term);
            if (mTotalFreq.TryGetValue(termId, out count))
            {
                return count;
            }
            return 0;
        }

        public int TotalDocCount
        {
            get { return mTotalDocCount; }
        }

        public IPostings Postings
        {
            get { return mPostings; }
            set { mPostings = value; }
        }
    }
}
