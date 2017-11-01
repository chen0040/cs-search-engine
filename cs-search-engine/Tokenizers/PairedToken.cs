using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Tokenizers
{
    public class PairedToken
    {
        protected string mWord = "";
        protected string mTag = "";

        public string Word
        {
            get { return mWord; }
            set { mWord = value; }
        }

        public string Tag
        {
            get { return mTag; }
            set { mTag = value; }
        }

        public override int GetHashCode()
        {
            int hash = mWord.GetHashCode();
            hash = hash * 31 + mTag.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            PairedToken rhs = obj as PairedToken;
            return mWord == rhs.mWord && mTag == rhs.mTag;
        }
    }
}
