using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SearchEngine.Indexing;
using SearchEngine.Tokenizers;
using SearchEngine.VSM;
using SearchEngine.Filters;
using System.IO;

namespace SearchEngine
{
    public class EmbedableSearchEngine
    {
        protected InvertedIndexer mIndexer = new InvertedIndexer();
        protected ITokenizer mTokenizer = new EnglishTokenizer();
        protected IRanking mRanker = new BM25Ranking();
        protected IPostings mPostings = new InMemoryPostings();
        protected List<IFilter> mFilters = new List<IFilter>();
        protected string mIndexerDirPath = "Indexer";

        public List<IFilter> Filters
        {
            get { return mFilters; }
        }

        public virtual void Save()
        {
            if (!string.IsNullOrEmpty(mIndexerDirPath))
            {
                if(!Directory.Exists(mIndexerDirPath))
                {
                    Directory.CreateDirectory(mIndexerDirPath);
                }
            }

            string filepath_docCount = Path.Combine(mIndexerDirPath, "docCount.csv");
            string filepath_bagsOfWords = Path.Combine(mIndexerDirPath, "bagsOfWords.csv");
            string filepath_config = Path.Combine(mIndexerDirPath, "config.xml");
            mIndexer.Save(filepath_docCount, filepath_bagsOfWords, filepath_config);
        }

        public virtual void Load()
        {
            if (!string.IsNullOrEmpty(mIndexerDirPath))
            {
                if (Directory.Exists(mIndexerDirPath))
                {
                    string filepath_docCount = Path.Combine(mIndexerDirPath, "docCount.csv");
                    string filepath_bagsOfWords = Path.Combine(mIndexerDirPath, "bagsOfWords.csv");
                    string filepath_config = Path.Combine(mIndexerDirPath, "config.xml");
                    mIndexer.Load(filepath_docCount, filepath_bagsOfWords, filepath_config);
                }
            }
        }

        public virtual void Clear()
        {
            if (!string.IsNullOrEmpty(mIndexerDirPath))
            {
                if (Directory.Exists(mIndexerDirPath))
                {
                    string filepath_docCount = Path.Combine(mIndexerDirPath, "docCount.csv");
                    string filepath_bagsOfWords = Path.Combine(mIndexerDirPath, "bagsOfWords.csv");
                    string filepath_config = Path.Combine(mIndexerDirPath, "config.xml");

                    mIndexer.Clear(filepath_docCount, filepath_bagsOfWords, filepath_config);
                }
            }
        }

        public IPostings Postings
        {
            get { return mPostings; }
            set
            {
                mPostings = value;
                mIndexer.Postings = mPostings;
            }
        }

        public EmbedableSearchEngine()
        {
            mIndexer.Postings = mPostings;
            mFilters.Add(new LCasing());
        }

        public IRanking RetrievalFunction
        {
            get { return mRanker; }
            set { mRanker = value; }
        }

        public void AddDoc(string docContent, int docId)
        {
            ITokenizer doc = mTokenizer.Clone();
            doc.Tokens = doc.Tokenize(docContent);
            ApplyFilters(doc);
            mIndexer.Index(doc, docId);
        }

        public void AddDoc(List<string> docFields, int docId)
        {
            ITokenizer doc = Transform(docFields);
            ApplyFilters(doc);
            mIndexer.Index(doc, docId);
        }

        public void ApplyFilters(ITokenizer doc)
        {
            if (mFilters.Count > 0)
            {
                List<string> words = doc.Tokens.ToList();
                foreach (IFilter f in mFilters)
                {
                    words = f.Filter(words);
                }
                doc.Tokens = words.ToArray();
            }
        }

        protected ITokenizer Transform(List<string> docFields)
        {
            ITokenizer doc2 = mTokenizer.Clone();
            doc2.Clear();
            int fieldCount = docFields.Count;
            List<string> result = new List<string>();
            for (int i = 0; i < fieldCount; ++i)
            {
                string[] fields = doc2.Tokenize(docFields[i]);
                HashSet<string> words = new HashSet<string>();
                foreach (string word in fields)
                {
                    words.Add(word);
                }
                foreach (string word in words)
                {
                    result.Add(word);
                }
            }
            doc2.Tokens = result.ToArray();
            return doc2;
        }

        public int[] Search(string queryContent, int resultCount, out double[] scores)
        {
            ITokenizer query = mTokenizer.Clone();
            query.Tokens = query.Tokenize(queryContent);
            
            ApplyFilters(query);

            return mIndexer.Search(query, mRanker, resultCount, out scores);
        }
    }
}
