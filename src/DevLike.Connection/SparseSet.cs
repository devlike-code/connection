using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connection
{
    using System.Collections;
    using System.Collections.Generic;

    public sealed class SparseSet : IEnumerable<int>
    {
        private readonly int MaxSize;
        private int Size;           
        private readonly int[] DenseArray;
        private readonly int[] SparseArray;

        public SparseSet(int maxValue)
        {
            MaxSize = maxValue + 1;
            Size = 0;
            DenseArray = new int[MaxSize];
            SparseArray = new int[MaxSize];
        }

        public void Add(int value)
        {
            if (value >= 0 && value < MaxSize && !Contains(value))
            {
                DenseArray[Size] = value;     
                SparseArray[value] = Size;    
                Size++;
            }
        }

        public void Remove(int value)
        {
            if (Contains(value))
            {
                DenseArray[SparseArray[value]] = DenseArray[Size - 1];                                                
                SparseArray[DenseArray[Size - 1]] = SparseArray[value];                                                
                Size--;
            }
        }

        public bool Contains(int value)
        {
            if (value >= MaxSize || value < 0)
                return false;
            else
                return SparseArray[value] < Size && DenseArray[SparseArray[value]] == value;
        }

        public void Clear()
        {
            Size = 0;
        }

        public int Count
        {
            get { return Size; }
        }

        public IEnumerator<int> GetEnumerator()
        {
            var i = 0;
            while (i < Size)
            {
                yield return DenseArray[i];
                i++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
