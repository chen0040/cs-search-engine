using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Indexing
{
    public class PostingData
    {
        protected int mTermID;
        public int TermID
        {
            get { return mTermID; }
            set { mTermID = value; }
        }

        protected int mDocID;
        public int DocID
        {
            get { return mDocID; }
            set { mDocID = value; }
        }

        protected int mDocLen;
        public int DocLen
        {
            get { return mDocLen; }
            set { mDocLen = value; }
        }

        public List<int> mPositions = new List<int>();
        public List<int> Positions
        {
            get { return mPositions; }
            set { mPositions = value; }
        }
    }
}
