using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Helpers
{
    public class MinPQEntry<ValueType>
    {
        public ValueType Value;
        public double Key;

        public MinPQEntry(double key, ValueType value)
        {
            Key = key;
            Value = value;
        }
    }

    public class MinPQ<ValueType>
    {

        private int mCapacity = 2;
        private MinPQEntry<ValueType>[] mData = null;
        private int mN = 0;

        public MinPQ(int capacity = 2)
        {
            mCapacity = capacity;
            mData = new MinPQEntry<ValueType>[capacity];
        }

        public int Count
        {
            get { return mN; }
        }

        public bool IsEmpty
        {
            get { return mN == 0; }
        }

        public void Add(double key, ValueType value)
        {
            mData[mN++] = new MinPQEntry<ValueType>(key, value);
            SwimUp();
            if (mN == mData.Length)
            {
                Resize(mData.Length * 2);
            }
        }

        private void SwimUp()
        {
            int i = mN;
            while (i > 1 && mData[i/2 - 1].Key > mData[i-1].Key)
            {
                MinPQEntry<ValueType> temp = mData[i/2 - 1];
                mData[i/2 - 1] = mData[i-1];
                mData[i-1] = temp;
                i = i / 2;
            }
        }

        public MinPQEntry<ValueType> DeleteMin()
        {
            if(mN==0)
            {
                throw new IndexOutOfRangeException();
            }
            MinPQEntry<ValueType> minEntry = mData[0];
            if(mN==1) 
            {
                mN=0;
                return minEntry;
            }
            mData[0] = mData[mN-1];
            mN--;
            Sink();
            if (mN < mData.Length / 4)
            {
                Resize(mData.Length / 2);
            }
            return minEntry;
        }

        private void Resize(int newSize)
        {
            MinPQEntry<ValueType>[] S = new MinPQEntry<ValueType>[newSize];
            for (int i = 0; i < mN; ++i)
            {
                S[i] = mData[i];
            }
            mData = S;
        }

        private void Sink()
        {
            int i = 1;
            while (i * 2 <= mN)
            {
                int j = i * 2;
                if (j + 1 <= mN && mData[j - 1].Key > mData[j].Key)
                {
                    j++;
                }
                if (mData[i - 1].Key <= mData[j-1].Key)
                {
                    break;
                }
                MinPQEntry<ValueType> temp = mData[i - 1];
                mData[i - 1] = mData[j - 1];
                mData[j - 1] = temp;
                i = j;
            }
        }

        public void Clear()
        {
            mData = new MinPQEntry<ValueType>[mCapacity];
            mN = 0;
        }

        public MinPQEntry<ValueType> Peek()
        {
            return mData[0];
        }
    }
}
